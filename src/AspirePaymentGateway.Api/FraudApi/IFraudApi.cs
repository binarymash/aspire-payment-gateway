using Refit;

namespace AspirePaymentGateway.Api.FraudApi
{
    public interface IFraudApi
    {
        [Post("/screening")]
        Task<ScreeningResponse> DoScreening(ScreeningRequest request, CancellationToken cancellationToken);
    }
}
