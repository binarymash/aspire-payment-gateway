using AspirePaymentGateway.Api.Events;
using OneOf;

namespace AspirePaymentGateway.Api.Storage
{
    public interface IPaymentEventRepository
    {
        Task<OneOf<StorageOk, StorageError>> SaveAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IPaymentEvent;

        Task<OneOf<IEnumerable<IPaymentEvent>, StorageError>> GetAsync(string paymentId, CancellationToken cancellationToken);
    }
}
