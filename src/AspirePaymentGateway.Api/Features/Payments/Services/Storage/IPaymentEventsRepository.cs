using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using BinaryMash.Extensions.Results;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Storage
{
    public interface IPaymentEventsRepository
    {
        Task<Result<IList<PaymentEvent>>> GetAsync(string paymentId, CancellationToken cancellationToken);

        Task<Result> SaveAsync(IList<PaymentEvent> paymentEvents, CancellationToken cancellationToken);
    }
}
