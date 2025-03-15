using AspirePaymentGateway.Api.Events;
using OneOf;

namespace AspirePaymentGateway.Api.Storage
{
    public interface IPaymentRepository
    {
        Task<OneOf<StorageOk, StorageError>> SavePaymentRequestAsync(string paymentId, PaymentRequest paymentRequest, CancellationToken cancellationToken);

        Task<OneOf<StorageOk, StorageError>> SavePaymentResultAsync(string paymentId, string paymentStatus, CancellationToken cancellationToken);

        //Task<OneOf<IEnumerable<PaymentEvent, StorageError>>> GetPaymentEventsAsync(string paymentId, CancellationToken cancellationToken);
    }
}
