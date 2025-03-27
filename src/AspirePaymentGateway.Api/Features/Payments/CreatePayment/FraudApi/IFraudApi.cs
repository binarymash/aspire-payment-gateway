using Refit;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi.Contracts;

namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi
{
    public interface IFraudApi
    {
        [Post("/screening")]
        Task<ScreeningResponse> DoScreening(ScreeningRequest request, CancellationToken cancellationToken);
    }
}
