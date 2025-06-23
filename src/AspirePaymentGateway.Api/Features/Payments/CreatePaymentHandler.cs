using System.Diagnostics;
using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.BankApi;
using AspirePaymentGateway.Api.Features.Payments.Services.FraudApi;
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
        IFraudApi fraudApi,
        IBankApi bankApi,
        BusinessMetrics metrics,
        IDateTimeProvider dateTimeProvider,
        ActivitySource activitySource)
    {
        public async Task<IResult> PostPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            var result = await RunDomainWorkflowAsync(paymentRequest, cancellationToken);

            if (result.IsSuccess)
            {
                // 201 created
                return Results.Created($"/payments/{result.Value.Id}", Contracts.MapPaymentResponse(result.Value));
            }

            return result.ErrorDetail switch
            {
                // 400 bad request
                Errors.ValidationError => Results.ValidationProblem(errors: (result.ErrorDetail as Errors.ValidationError)!.ValidationResult.ToDictionary()),

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

            if (result.IsFailure)
            {
                LogErrorDetail(result.ErrorDetail);
            }

            return result;
        }

        // Activities

        private async Task<Result<Payment>> AcceptPaymentRequestAsync(PaymentRequest paymentRequest, CancellationToken ct)
        {
            using var activity = activitySource.StartActivity("Accepting payment", ActivityKind.Internal);

            paymentRequest ??= new(null!, null!);

            var validationResult = await validator.ValidateAsync(paymentRequest, ct);
            activity?.AddEvent(new ActivityEvent("Request validated"));

            if (!validationResult.IsValid)
            {
                return Result.Error<Payment>(new Errors.ValidationError(validationResult));
            }

            activity?.AddBaggage("funky", "whatsit");

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

            activity?.AddTag("activity-tag", "jeepers");

            var result = await session.CommitAsync(payment, ct);

            if (result.IsSuccess)
            {
                metrics.RecordPaymentRequestAccepted();
                LogPaymentAccepted(payment.Id);
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Error);
            }

            return result;
        }

        private async Task<Result<Payment>> ScreenPaymentAsync(Payment payment, CancellationToken ct)
        {
            using var activity = activitySource.StartActivity("Screening payment", ActivityKind.Internal);

            var screeningRequest = new Services.FraudApi.Contracts.ScreeningRequest()
            {
                CardNumber = payment.Card.CardNumber,
                CardHolderName = payment.Card.CardHolderName,
                ExpiryMonth = payment.Card.Expiry.Month,
                ExpiryYear = payment.Card.Expiry.Year
            };

            Result<Payment> result = payment;

            try
            {
                var screeningResponse = await fraudApi.DoScreeningAsync(screeningRequest, ct);

                payment.RecordScreeningResponse(screeningResponse.Accepted);
            }
            catch (Exception ex)
            {
                result = Result.Error<Payment>(new Errors.FraudApiExceptionError(ex));
            }

            if (result.IsSuccess)
            {
                result = await session.CommitAsync(payment, ct);
            }

            if (result.IsFailure)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
            }

            return result;
        }

        private async Task<Result<Payment>> AuthorisePaymentAsync(Payment payment, CancellationToken ct)
        {
            using var activity = activitySource.StartActivity("Authorising payment", ActivityKind.Internal);

            var authorisationRequest = new Services.BankApi.Contracts.AuthorisationRequest
            {
                AuthorisationRequestId = Guid.NewGuid().ToString(),
                Pan = payment.Card.CardNumber,
                CardHolderFullName = payment.Card.CardHolderName,
                Cvv = payment.Card.CVV,
                ExpiryMonth = payment.Card.Expiry.Month,
                ExpiryYear = payment.Card.Expiry.Year,
                Amount = payment.Amount.ValueInMinorUnits,
                CurrencyCode = payment.Amount.CurrencyCode
            };

            Result<Payment> result = payment;

            try
            {
                var authorisationResponse = await bankApi.AuthoriseAsync(authorisationRequest, ct);

                activity?.AddEvent(new ActivityEvent("Request authorised"));

                payment.RecordAuthorisationResponse(
                    authorisationResponse.AuthorisationRequestId,
                    authorisationResponse.Authorised,
                    authorisationResponse.AuthorisationCode);
            }
            catch (Exception ex)
            {
                result = Result.Error<Payment>(new Errors.BankApiExceptionError(ex));
            }

            if (result.IsSuccess)
            {
                result = await session.CommitAsync(payment, ct);
                metrics.RecordPaymentFate(payment.Status);
            }

            if (result.IsFailure)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
            }

            return result;
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

            metrics.RecordPaymentFate(payment.Status, payment.DeclineReason);

            var result = await session.CommitAsync(payment, ct);

            if (result.IsFailure)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
            }

            return result;
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred: {ErrorDetail}")]
        partial void LogErrorDetail(ErrorDetail errorDetail);

        [LoggerMessage(Level = LogLevel.Information, Message = "Payment accepted: {PaymentId}")]
        partial void LogPaymentAccepted(string paymentId);
    }
}
