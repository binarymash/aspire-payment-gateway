using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using BinaryMash.Extensions.Results;

namespace AspirePaymentGateway.Api.Storage.InMemory
{
    public class InMemoryPaymentEventRepository : ISavePayment, IPaymentEventsRepository
    {
        readonly Dictionary<string, List<IPaymentEvent>> _store = [];

        public async Task<Result<IList<IPaymentEvent>>> GetAsync(string paymentId, CancellationToken cancellationToken)
        {
            if (!_store.TryGetValue(paymentId, out var events))
            {
                return Result.Error<IList<IPaymentEvent>>(new Errors.PaymentNotFoundError());
            }

            return await Task.FromResult(events);
        }

        public async Task<Result> SaveAsync(IList<IPaymentEvent> paymentEvents, CancellationToken cancellationToken)
        {
            foreach (var paymentEvent in paymentEvents)
            {
                if (!_store.TryGetValue(paymentEvent.Id, out var storedPaymentEvents))
                {
                    storedPaymentEvents = [];
                    _store[paymentEvent.Id] = storedPaymentEvents;
                }
                storedPaymentEvents.Add(paymentEvent);
            }

            return await Task.FromResult(new OkResult());
        }

        public void Clear()
        {
            _store.Clear();
        }
    }
}
