using System.Diagnostics;
using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.Bank;
using AspirePaymentGateway.Api.Features.Payments.Services.Fraud;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using AspirePaymentGateway.Api.Telemetry;
using BinaryMash.Extensions.DateTime;
using BinaryMash.Extensions.Results;
using FluentValidation;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;

namespace AspirePaymentGateway.Api.Features.Payments
{
    // HTTP concerns

    public partial class CreatePaymentHandler(
        ILogger<CreatePaymentHandler> logger,
        IValidator<PaymentRequest> validator,
        PaymentSession session,
        IFraudService fraudService,
        IBankService bankService,
        BusinessMetrics metrics,
        IDateTimeProvider dateTimeProvider,
        ActivitySource activitySource)
    {
        public async Task<IResult> PostPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            var result = await RunDomainWorkflowAsync(paymentRequest, cancellationToken);

            return result.Match(
                onSuccess: MapPaymentCreatedResponse201,
                onFailure: MapFailureResponse);
        }

        private static IResult MapPaymentCreatedResponse201(Result<Payment> success)
        {
            return Results.Created($"/payments/{success.Value.Id}", Contracts.MapPaymentResponse(success.Value));
        }

        private static IResult MapFailureResponse(Result<Payment> failure)
        {
            return failure.ErrorDetail switch
            {
                // 400 bad request
                Errors.ValidationError => Results.ValidationProblem(errors: (failure.ErrorDetail as Errors.ValidationError)!.ValidationResult.ToDictionary()),

                // 500 internal server error
                _ => Results.Problem(),
            };
        }

        // Domain workflow

        private async Task<Result<Payment>> RunDomainWorkflowAsync(PaymentRequest paymentRequest, CancellationToken ct)
        {
            // request validation and acceptance
            var result = await AcceptPaymentRequestAsync(paymentRequest, ct);

            if (result.IsFailure && result.ErrorDetail is Errors.ValidationError)
            {
                metrics.RecordPaymentRequestRejected();
            }

            // screening
            if (result.IsSuccess)
            {
                result = await ScreenPaymentAsync(result.Value, ct);
                if (result.IsSuccess && result.Value.ScreeningStatus != ScreeningStatus.Passed)
                {
                    result = await DeclinePaymentAsync(result.Value, "Fraud screening failed", ct);
                }
            }

            // authorisation
            if (result.IsSuccess && result.Value.ScreeningStatus == ScreeningStatus.Passed)
            {
                result = await AuthorisePaymentAsync(result.Value, ct);
                if (result.IsSuccess && result.Value.Status != PaymentStatus.Authorised)
                {
                    result = await DeclinePaymentAsync(result.Value, "Fraud screening failed", ct);
                }
            }

            return result.RecordActivityResult(
                onSuccess: _ =>
                {
                    metrics.RecordPaymentFate(result.Value.Status, result.Value.DeclineReason);
                    LogPaymentProcessingCompleted(result.Value.Id, result.Value.Status, result.Value.DeclineReason);
                },
                onFailure: _ =>
                {
                    LogPaymentProcessingFailed(result.Value?.Id, result.ErrorDetail);
                });
        }

        // Activities

        private async Task<Result<Payment>> AcceptPaymentRequestAsync(PaymentRequest paymentRequest, CancellationToken ct)
        {
            using var activity = activitySource.StartActivity("Accepting payment request", ActivityKind.Internal);

            paymentRequest ??= new(null!, null!);

            var validationResult = await validator.ValidateAsync(paymentRequest, ct);
            activity?.AddEvent(new ActivityEvent("Request validated"));

            if (!validationResult.IsValid)
            {
                return Result.Failure<Payment>(new Errors.ValidationError(validationResult));
            }

            var payment = Payment.Create(
                paymentId: $"pay_{Guid.NewGuid()}",
                amountInMinorUnits: paymentRequest.Payment.Amount,
                currencyIsoCode: paymentRequest.Payment.CurrencyCode,
                cardNumber: paymentRequest.Card.CardNumber,
                cardHolderName: paymentRequest.Card.CardHolderName,
                cvv: paymentRequest.Card.CVV,
                expiryMonth: paymentRequest.Card.Expiry.Month,
                expiryYear: paymentRequest.Card.Expiry.Year
                );

            var result = await session.CommitAsync(payment, ct);

            return result.RecordActivityResult(
                onSuccess: _ =>
                {
                    metrics.RecordPaymentRequestAccepted();
                    LogPaymentRequestAccepted(payment.Id);
                });
        }

        private async Task<Result<Payment>> ScreenPaymentAsync(Payment payment, CancellationToken ct)
        {
            using var activity = activitySource.StartActivity("Screening payment", ActivityKind.Internal);

            var screeningResult = await fraudService.ScreenPaymentAsync(payment, ct);

            var result = await screeningResult
                .Then(screeningResult => payment.RecordScreeningResponse(screeningResult.Value))
                .ThenAsync(async _ => await session.CommitAsync(payment, ct));

            return result.RecordActivityResult(onSuccess: _ => LogPaymentScreened(payment.Id));
        }

        private async Task<Result<Payment>> AuthorisePaymentAsync(Payment payment, CancellationToken ct)
        {
            using var activity = activitySource.StartActivity("Authorising payment", ActivityKind.Internal);

            var authoristionDetails = await bankService.AuthorisePaymentAsync(payment, ct);

            var result = await authoristionDetails
                .Then(authorisationResponse => payment.RecordAuthorisationDetails(authorisationResponse.Value))
                .ThenAsync(async _ => await session.CommitAsync(payment, ct));

            return result.RecordActivityResult(onSuccess: _ => LogPaymentAuthorisationCompleted(payment.Id, authoristionDetails.Value));
        }

        private async Task<Result<Payment>> DeclinePaymentAsync(Payment payment, string reason, CancellationToken ct)
        {
            using var activity = activitySource.StartActivity("Declining payment", ActivityKind.Internal);

            payment.Apply(new PaymentDeclinedEvent
            {
                Id = $"evt_{Guid.NewGuid()}",
                PaymentId = payment.Id,
                OccurredAt = dateTimeProvider.UtcNowAsString,
                Reason = reason
            });

            var result = await session.CommitAsync(payment, ct);

            return result.RecordActivityResult(onSuccess: _ => LogPaymentDeclined(payment.Id, reason));
        }

        [LoggerMessage(Level = LogLevel.Debug, Message = "Payment request accepted: {PaymentId}")]
        partial void LogPaymentRequestAccepted(string paymentId);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Payment screened: {PaymentId}")]
        partial void LogPaymentScreened(string paymentId);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Payment authorisation completed: {PaymentId}, {AuthorisationDetails}")]
        partial void LogPaymentAuthorisationCompleted(string paymentId, AuthorisationDetails authorisationDetails);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Payment declined: {PaymentId}, {DeclineReason}")]
        partial void LogPaymentDeclined(string paymentId, string declineReason);

        [LoggerMessage(Level = LogLevel.Information, Message = "Payment processing completed: {PaymentId}, {PaymentStatus}, {DeclineReason}")]
        partial void LogPaymentProcessingCompleted(string paymentId, string paymentStatus, string? declineReason);

        [LoggerMessage(Level = LogLevel.Error, Message = "Payment processing failed: {PaymentId}, {ErrorDetail}")]
        partial void LogPaymentProcessingFailed(string? paymentId, ErrorDetail errorDetail);

    }
}
