using Refit;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Fraud.HttpApi;

[Headers("Authorization: Bearer")]
public interface IFraudApi
{
    [Post("/screening")]
    Task<Contracts.ScreeningResponse> DoScreeningAsync(Contracts.ScreeningRequest request, CancellationToken cancellationToken);
}
