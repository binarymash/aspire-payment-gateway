namespace AspirePaymentGateway.Api.FraudApi
{
    public interface IFraudApi
    {
        [Refit.Post("/screening")]
        Task<ScreeningResponse> DoScreening(ScreeningRequest request, CancellationToken cancellationToken);
    }
}
