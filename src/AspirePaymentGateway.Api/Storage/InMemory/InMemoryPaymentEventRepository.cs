using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using BinaryMash.Extensions.Results;

namespace AspirePaymentGateway.Api.Storage.InMemory
{
    public class InMemoryPaymentEventRepository : ISavePayment, IPaymentEventsRepository
    {
        readonly Dictionary<string, List<PaymentEvent>> _store = [];

        public async Task<Result<IList<PaymentEvent>>> GetAsync(string paymentId, CancellationToken cancellationToken)
        {
            if (!_store.TryGetValue(paymentId, out var events))
            {
                return Result.Error<IList<PaymentEvent>>(new Errors.PaymentNotFoundError());
            }

            return await Task.FromResult(events);
        }

        public async Task<Result> SaveAsync(IList<PaymentEvent> paymentEvents, CancellationToken cancellationToken)
        {
            foreach (var paymentEvent in paymentEvents)
            {
                if (!_store.TryGetValue(paymentEvent.PaymentId, out var storedPaymentEvents))
                {
                    storedPaymentEvents = [];
                    _store[paymentEvent.PaymentId] = storedPaymentEvents;
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
