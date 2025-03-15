namespace AspirePaymentGateway.Api.FraudApi
{
    public record ScreeningResponse
    {
        public bool Accepted { get; init; }
    }
}