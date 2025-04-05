using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.Extensions.Results;
using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.BankApi;
using AspirePaymentGateway.Api.Features.Payments.Services.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using AspirePaymentGateway.Api.Telemetry;
using FluentValidation;
using System.Diagnostics;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;

namespace AspirePaymentGateway.Api.Features.Payments
{
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
            var result = await CreatePaymentWorkflow(paymentRequest, cancellationToken);

            if (result.IsSuccess)
            {
                return Results.Created($"/payments/{result.Value.Id}", Contracts.MapPaymentResponse(result.Value));
            }

            if (result.ErrorDetail is Errors.ValidationError)
            {
                return Results.ValidationProblem(errors: (result.ErrorDetail as Errors.ValidationError)!.ValidationResult.ToDictionary());
            }

            return Results.Problem(detail: result.ErrorDetail.ToString());
        }

        private async Task<Result<Payment>> CreatePaymentWorkflow(PaymentRequest paymentRequest, CancellationToken ct)
        {
            // request validation and acceptance
            var result = await AcceptPaymentRequest(paymentRequest, ct);
            if (result.IsFailure)
            {
                if (result.ErrorDetail is Errors.ValidationError)
                {
                    metrics.RecordPaymentRequestRejected();
                }
            }

            // screening
            if (result.IsSuccess)
            {
                result = await ScreenPayment(result.Value, ct);
                if (result.IsSuccess && result.Value.ScreeningStatus != ScreeningStatus.Passed)
                {
                    result = await DeclinePayment(result.Value, "Fraud screening failed", ct);
                }
            }

            // authorisation
            if (result.IsSuccess && result.Value.ScreeningStatus == ScreeningStatus.Passed)
            {
                result = await AuthorisePayment(result.Value, ct);
                if (result.IsSuccess && result.Value.Status != PaymentStatus.Authorised)
                {
                    result = await DeclinePayment(result.Value, "Fraud screening failed", ct);
                }
            }

            if (!result.IsSuccess)
            {
                LogErrorDetail(result.ErrorDetail);
            }

            return result;
        }

        public async Task<Result<Payment>> AcceptPaymentRequest(PaymentRequest paymentRequest, CancellationToken ct)
        {
            using (Activity.Current = activitySource.StartActivity("Accepting payment", ActivityKind.Internal))
            {
                var validationResult = await validator.ValidateAsync(paymentRequest ?? new(null!, null!), ct);
                Activity.Current?.AddEvent(new ActivityEvent("Request validated"));

                if (!validationResult.IsValid)
                {
                    return Result.Error<Payment>(new Errors.ValidationError(validationResult));
                }
                Activity.Current?.AddBaggage("funky", "whatsit");
                Payment payment = new();

                payment.Apply(new PaymentRequestedEvent
                {
                    Id = $"pay_{Guid.NewGuid()}",
                    OccurredAt = dateTimeProvider.UtcNowAsString,
                    Amount = paymentRequest.Payment.Amount,
                    Currency = paymentRequest.Payment.CurrencyCode,
                    CardNumber = paymentRequest.Card.CardNumber,
                    CardHolderName = paymentRequest.Card.CardHolderName,
                    Cvv = paymentRequest.Card.CVV,
                    ExpiryMonth = paymentRequest.Card.Expiry.Month,
                    ExpiryYear = paymentRequest.Card.Expiry.Year
                });

                Activity.Current?.AddTag("activity-tag", "jeepers");

                var result = await session.CommitAsync(payment, ct);

                metrics.RecordPaymentRequestAccepted();
                LogPaymentAccepted(payment.Id);

                return result;
            }
        }

        public async Task<Result<Payment>> ScreenPayment(Payment payment, CancellationToken ct)
        {
            using (Activity.Current = activitySource.StartActivity("Screening payment", ActivityKind.Internal))
            {
                var screeningRequest = new Services.FraudApi.Contracts.ScreeningRequest()
                {
                    CardNumber = payment.Card.CardNumber,
                    CardHolderName = payment.Card.CardHolderName,
                    ExpiryMonth = payment.Card.Expiry.Month,
                    ExpiryYear = payment.Card.Expiry.Year
                };

                Result<Payment> result = null!;

                try
                {
                    var screeningResponse = await fraudApi.DoScreening(screeningRequest, ct);
                    payment.Apply(new PaymentScreenedEvent { Id = payment.Id, OccurredAt = dateTimeProvider.UtcNowAsString, ScreeningAccepted = screeningResponse.Accepted });
                    result = await session.CommitAsync(payment, ct);
                }
                catch (Exception ex)
                {
                    result = Result.Error<Payment>(new Errors.ExceptionError("SOP-whatever", "Fraud API failed", ex));
                }

                if (result.IsFailure)
                {
                    Activity.Current?.SetStatus(ActivityStatusCode.Error);
                }

                return result;
            }
        }

        public async Task<Result<Payment>> AuthorisePayment(Payment payment, CancellationToken ct)
        {
            using (Activity.Current = activitySource.StartActivity("Authorising payment", ActivityKind.Internal))
            {
                var authorisationRequest = new Services.BankApi.Contracts.AuthorisationRequest()
                {
                };

                var authorisationResponse = await bankApi.AuthoriseAsync(authorisationRequest, ct);
                Activity.Current?.AddEvent(new ActivityEvent("Request authorised"));

                payment.Apply(new PaymentAuthorisedEvent
                {
                    Id = payment.Id,
                    OccurredAt = dateTimeProvider.UtcNowAsString,
                    AuthorisationCode = "abc"
                });

                var result = await session.CommitAsync(payment, ct);

                metrics.RecordPaymentFate(payment.Status);

                if (result.IsFailure)
                {
                    Activity.Current?.SetStatus(ActivityStatusCode.Error);
                }

                return result;
            }
        }

        public async Task<Result<Payment>> DeclinePayment(Payment payment, string reason, CancellationToken ct)
        {
            using (Activity.Current = activitySource.StartActivity("Declining payment", ActivityKind.Internal))
            {
                payment.Apply(new PaymentDeclinedEvent
                {
                    Id = payment.Id,
                    OccurredAt = dateTimeProvider.UtcNowAsString,
                    Reason = reason
                });

                metrics.RecordPaymentFate(payment.Status, payment.DeclineReason);

                var result = await session.CommitAsync(payment, ct);

                if (result.IsFailure)
                {
                    Activity.Current?.SetStatus(ActivityStatusCode.Error);
                }

                return result;
            }
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred: {ErrorDetail}")]
        partial void LogErrorDetail(ErrorDetail errorDetail);

        [LoggerMessage(Level = LogLevel.Information, Message = "Payment accepted: {PaymentId}")]
        partial void LogPaymentAccepted(string paymentId);
    }
}
