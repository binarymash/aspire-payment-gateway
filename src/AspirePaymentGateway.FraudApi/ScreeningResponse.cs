namespace AspirePaymentGateway.Api.FraudApi
{
    public record ScreeningResponse
    {
        public int SomeNumber { get; init; }
        public bool Accepted { get; init; }
    }
}