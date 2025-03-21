using Refit;

namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi
{
    public interface IFraudApi
    {
        [Post("/screening")]
        Task<ScreeningResponse> DoScreening(ScreeningRequest request, CancellationToken cancellationToken);
    }
}
