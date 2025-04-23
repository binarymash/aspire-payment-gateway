using Refit;

namespace AspirePaymentGateway.Api.Features.Payments.Services.FraudApi
{
    [Headers("Authorization: Bearer")]
    public interface IFraudApi
    {
        [Post("/screening")]
        Task<Contracts.ScreeningResponse> DoScreeningAsync(Contracts.ScreeningRequest request, CancellationToken cancellationToken);
    }
}
