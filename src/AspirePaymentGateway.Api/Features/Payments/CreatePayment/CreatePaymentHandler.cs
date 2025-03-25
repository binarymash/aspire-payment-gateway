using System.Diagnostics;
using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.Extensions.Results;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.BankApi;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.EventStore;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.Events;
using AspirePaymentGateway.Api.Telemetry;
using FluentValidation;
using FluentResults;
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
            // validation
            var result = await ValidateRequestAsync(paymentRequest, cancellationToken);
            if (result.IsFailed)
            {
                return Results.ValidationProblem(
                    //errors: paymentRequested.HasError<ValidationFailure>(ValidationResult.ToDictionary(),
                    instance: $"{context.Request.Method} {context.Request.Path}"
                );
            }

            //fraud checks
            result = await PerformScreening(result.Value, cancellationToken);
            if (result.IsFailed)
            {
                return Results.Created($"/payments/{result.Value.Id}", new PaymentResponse(result.Value.Id, PaymentStatus.Declined));
            }

            // authorisation
            result = await Authorise(result.Value, cancellationToken);

            return Results.Created($"/payments/{result.Value.Id}", new PaymentResponse(result.Value.Id, PaymentStatus.Authorised));
        }








        private async Task<Result<Payment>> ValidateRequestAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {

            var validationResult = await validator.ValidateAsync(paymentRequest);
            Activity.Current?.AddEvent(new ActivityEvent("Request validated"));

            if (!validationResult.IsValid)
            {
                metrics.RecordPaymentRequestRejected();

                return Result.Fail<PaymentRequestedEvent>(new ValidationError
                {
                    ValidationResult = validationResult
                });
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

            var payment = new Payment();
            payment.Apply(paymentRequested);

            await repository.SaveAsync(paymentRequested, cancellationToken);

            metrics.RecordPaymentRequestAccepted();

            return Result.Ok(payment);
        }

        private async Task<Result<Payment>> PerformScreening(Payment payment, CancellationToken cancellationToken)
        {
            var screeningRequest = new ScreeningRequest()
            {
                CardNumber = payment.CardNumber,
                CardHolderName = payment.CardHolderName,
                ExpiryMonth = payment.CardExpiry.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear
            };

            var screeningResponse = await fraudApi.DoScreening(screeningRequest, cancellationToken);
            Activity.Current?.AddEvent(new ActivityEvent("Request screened"));

            if (!screeningResponse.Accepted)
            {
                var paymentDeclined = new PaymentDeclinedEvent
                {
                    Id = payment.Id,
                    OccurredAt = dateTimeProvider.UtcNowAsString,
                    Reason = ""
                };

                payment.Apply(paymentDeclined);

                metrics.RecordPaymentFate(paymentDeclined.EventType, paymentDeclined.Reason);

                var saveResult = await repository.SaveAsync(paymentDeclined, cancellationToken);

                return Result.Fail(Errors.ScreeningFailure());
            }

            return Result.Ok(payment);
        }

        private async Task<Result<Payment>> Authorise(Payment payment, CancellationToken cancellationToken)
        {            
            var authorisationRequest = new AuthorisationRequest()
            {
            };

            var authorisationResponse = await bankApi.AuthoriseAsync(authorisationRequest, cancellationToken);
            Activity.Current?.AddEvent(new ActivityEvent("Request authorised"));

            var paymentAuthorised = new PaymentAuthorisedEvent
            {
                Id = payment.Id,
                OccurredAt = dateTimeProvider.UtcNowAsString,
                AuthorisationCode = "abc"
            };

            payment.Apply(paymentAuthorised);

            metrics.RecordPaymentFate(paymentAuthorised.EventType);

            await repository.SaveAsync(paymentAuthorised, cancellationToken);

            return Result.Ok(payment);
        }
    }
}
