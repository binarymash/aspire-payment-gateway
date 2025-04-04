using AspirePaymentGateway.Api.Extensions.Results;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Storage
{
    public interface IPaymentEventsRepository
    {
        Task<Result<IList<IPaymentEvent>>> GetAsync(string paymentId, CancellationToken cancellationToken);

        Task<Result> SaveAsync(IList<IPaymentEvent> paymentEvents, CancellationToken cancellationToken);
    }
}
