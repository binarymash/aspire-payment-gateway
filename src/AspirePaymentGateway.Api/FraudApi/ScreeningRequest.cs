namespace AspirePaymentGateway.Api.FraudApi
{
    public record ScreeningRequest
    {
        public required string CardNumber { get; init; }

        public required string CardHolderName { get; init; }

        public required int ExpiryMonth { get; init; }

        public required int ExpiryYear { get; init; }
    }
}