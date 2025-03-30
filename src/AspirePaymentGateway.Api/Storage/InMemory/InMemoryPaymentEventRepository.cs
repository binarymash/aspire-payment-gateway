using AspirePaymentGateway.Api.Features.Payments.CreatePayment.EventStore;
using AspirePaymentGateway.Api.Features.Payments.Events;
using AspirePaymentGateway.Api.Features.Payments.GetPayment.EventStore;
using OneOf;

namespace AspirePaymentGateway.Api.Storage.InMemory
{
    public class InMemoryPaymentEventRepository : ISavePaymentEvent, IGetPaymentEvent
    {
        readonly Dictionary<string, List<IPaymentEvent>> _store = new();

        public async Task<OneOf<IEnumerable<IPaymentEvent>, Exception>> GetAsync(string paymentId, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (!_store.TryGetValue(paymentId, out var events))
            {
                events = new List<IPaymentEvent>();
            }

            return events;
        }

        public async Task<OneOf<StorageOk, Exception>> SaveAsync<TEvent>(TEvent paymentEvent, CancellationToken cancellationToken) where TEvent : IPaymentEvent
        {
            await Task.CompletedTask;

            if (!_store.TryGetValue(paymentEvent.Id, out var events))
            {
                events = new List<IPaymentEvent>();
                _store[paymentEvent.Id] = events;
            }

            events.Add(paymentEvent);

            return new StorageOk();
        }
    }
}
