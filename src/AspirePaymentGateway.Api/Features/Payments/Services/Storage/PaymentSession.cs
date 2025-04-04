using AspirePaymentGateway.Api.Extensions.Results;
using AspirePaymentGateway.Api.Features.Payments.Domain;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Storage
{
    public class PaymentSession(IPaymentEventsRepository repository)
    {
        public async Task<Result<Payment>> GetAsync(string paymentId, CancellationToken ct)
        {
            Payment payment = new Payment();

            var result = await repository.GetAsync(paymentId, ct);

            if (result.IsFailure)
            {
                return new ErrorResult<Payment>(result.ErrorDetail);
            }

            if (result.Value.Count == 0)
            {
                return new ErrorResult<Payment>(new Errors.PaymentNotFoundError());
            }

            foreach (var paymentEvent in result.Value)
            {
                payment.Apply(paymentEvent);
            }

            payment.FlushUncommittedEvents();

            return payment;
        }

        public async Task<Result<Payment>> CommitAsync(Payment payment, CancellationToken ct)
        {
            var result = await repository.SaveAsync(payment.UncommittedEvents, ct);
            if (result.IsSuccess)
            {
                payment.FlushUncommittedEvents();
                return payment;
            }

            return new ErrorResult<Payment>(result.ErrorDetail);
        }
    }
}
