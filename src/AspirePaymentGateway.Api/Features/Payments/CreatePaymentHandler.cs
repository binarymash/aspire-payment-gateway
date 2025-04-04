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
        IDateTimeProvider dateTimeProvider)
    {
        public async Task<IResult> PostPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            // request validation

            var validationResult = await validator.ValidateAsync(paymentRequest ?? new(null!, null!), cancellationToken);
            Activity.Current?.AddEvent(new ActivityEvent("Request validated"));

            if (!validationResult.IsValid)
            {
                metrics.RecordPaymentRequestRejected();
                return Results.ValidationProblem(errors: validationResult.ToDictionary());
            }

            var result = await AcceptPaymentRequest(paymentRequest, cancellationToken);

            // screening
            if (result.IsSuccess)
            {
                result = await ScreenPayment(result.Value, cancellationToken);
                if (result.IsSuccess &&  result.Value.ScreeningStatus != ScreeningStatus.Passed)
                {
                    result = await DeclinePayment(result.Value, "Fraud screening failed", cancellationToken);
                }
            }

            // authorisation
            if (result.IsSuccess && result.Value.ScreeningStatus == ScreeningStatus.Passed)
            {
                result = await AuthorisePayment(result.Value, cancellationToken);
                if (result.IsSuccess && result.Value.Status != PaymentStatus.Authorised)
                {
                    result = await DeclinePayment(result.Value, "Fraud screening failed", cancellationToken);
                }
            }

            // nominal response
            if (result.IsSuccess)
            {
                return Results.Created($"/payments/{result.Value.Id}", Contracts.MapPaymentResponse(result.Value));
            }

            // error response
            LogErrorDetail(result.ErrorDetail);
            return Results.Problem(detail : result.ErrorDetail.ToString());
        }

        public async Task<Result<Payment>> AcceptPaymentRequest(PaymentRequest paymentRequest, CancellationToken ct)
        {
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

            var result = await session.CommitAsync(payment, ct);

            metrics.RecordPaymentRequestAccepted();
            LogPaymentAccepted(payment.Id);
            
            return result;
        }

        public async Task<Result<Payment>> ScreenPayment(Payment payment, CancellationToken ct)
        {
            var screeningRequest = new Services.FraudApi.Contracts.ScreeningRequest()
            {
                CardNumber = payment.Card.CardNumber,
                CardHolderName = payment.Card.CardHolderName,
                ExpiryMonth = payment.Card.Expiry.Month,
                ExpiryYear = payment.Card.Expiry.Year
            };

            var screeningResponse = await fraudApi.DoScreening(screeningRequest, ct);

            payment.Apply(new PaymentScreenedEvent { Id = payment.Id, OccurredAt = dateTimeProvider.UtcNowAsString, ScreeningAccepted = screeningResponse.Accepted });
            
            var result = await session.CommitAsync(payment, ct);

            Activity.Current?.AddEvent(new ActivityEvent("Request screened"));

            return result;
        }

        public async Task<Result<Payment>> AuthorisePayment(Payment payment, CancellationToken ct)
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

            return result;
        }

        public async Task<Result<Payment>> DeclinePayment(Payment payment, string reason, CancellationToken ct)
        {
            payment.Apply(new PaymentDeclinedEvent
            {
                Id = payment.Id,
                OccurredAt = dateTimeProvider.UtcNowAsString,
                Reason = reason
            });

            metrics.RecordPaymentFate(payment.Status, payment.DeclineReason);

            return await session.CommitAsync(payment, ct);
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred: {ErrorDetail}")]
        partial void LogErrorDetail(ErrorDetail errorDetail);

        [LoggerMessage(Level = LogLevel.Information, Message = "Payment accepted: {PaymentId}")]
        partial void LogPaymentAccepted(string paymentId);
    }
}
