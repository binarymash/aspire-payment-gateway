using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.BankApi;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.EventStore;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.Events;
using AspirePaymentGateway.Api.Telemetry;
using FluentValidation;
using System.Diagnostics;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.Contracts;

namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment
{
    public class CreatePaymentHandler(
        IValidator<PaymentRequest> validator,
        ISavePaymentEvent repository,
        IFraudApi fraudApi,
        IBankApi bankApi,
        BusinessMetrics metrics,
        IDateTimeProvider dateTimeProvider)
{
        public async Task<IResult> PostPaymentAsync(HttpContext context, PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            // request validation

            var validationResult = await validator.ValidateAsync(paymentRequest);
            Activity.Current?.AddEvent(new ActivityEvent("Request validated"));

            if (!validationResult.IsValid)
            {
                metrics.RecordPaymentRequestRejected();
                return Results.ValidationProblem(
                    errors: validationResult.ToDictionary(),
                    instance: $"{context.Request.Method} {context.Request.Path}");
            }

            var paymentRequested = new PaymentRequestedEvent
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
            };

            metrics.RecordPaymentRequestAccepted();
            var saveResult = await repository.SaveAsync(paymentRequested, cancellationToken);

            // fraud checks

            var screeningRequest = new ScreeningRequest()
            {
                CardNumber = paymentRequest.Card.CardNumber,
                CardHolderName = paymentRequest.Card.CardHolderName,
                ExpiryMonth = paymentRequest.Card.Expiry.Month,
                ExpiryYear = paymentRequest.Card.Expiry.Year
            };

            var screeningResponse = await fraudApi.DoScreening(screeningRequest, cancellationToken);
            Activity.Current?.AddEvent(new ActivityEvent("Request screened"));

            if (!screeningResponse.Accepted)
            {
                var paymentDeclined = new PaymentDeclinedEvent
                {
                    Id = paymentRequested.Id,
                    OccurredAt = dateTimeProvider.UtcNowAsString,
                    Reason = ""
                };

                metrics.RecordPaymentFate(paymentDeclined.EventType, paymentDeclined.Reason);

                saveResult = await repository.SaveAsync(paymentDeclined, cancellationToken);

                return Results.Created($"/payments/{paymentRequested.Id}", new PaymentResponse(paymentRequested.Id, PaymentStatus.Declined));
            }

            // authorisation

            var authorisationRequest = new AuthorisationRequest()
            {
            };

            var authorisationResponse = await bankApi.AuthoriseAsync(authorisationRequest, cancellationToken);
            Activity.Current?.AddEvent(new ActivityEvent("Request authorised"));

            var paymentAuthorised = new PaymentAuthorisedEvent
            {
                Id = paymentRequested.Id,
                OccurredAt = dateTimeProvider.UtcNowAsString,
                AuthorisationCode = "abc"
            };

            metrics.RecordPaymentFate(paymentAuthorised.EventType);

            saveResult = await repository.SaveAsync(paymentAuthorised, cancellationToken);

            return Results.Created($"/payments/{paymentRequested.Id}", new PaymentResponse(paymentRequested.Id, PaymentStatus.Authorised));
        }
    }
}
