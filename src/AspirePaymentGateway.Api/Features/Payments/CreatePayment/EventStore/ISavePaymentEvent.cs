using AspirePaymentGateway.Api.Features.Payments.Events;
using OneOf;

namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.EventStore
{
    public interface ISavePaymentEvent
    {
        Task<OneOf<StorageOk, Exception>> SaveAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IPaymentEvent;
    }
}
