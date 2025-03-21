using AspirePaymentGateway.Api.Features.Payments.Events;
using OneOf;

namespace AspirePaymentGateway.Api.Features.Payments.GetPayment.EventStore
{
    public interface IGetPaymentEvent
    {
        Task<OneOf<IEnumerable<IPaymentEvent>, Exception>> GetAsync(string paymentId, CancellationToken cancellationToken);
    }
}
