namespace AspirePaymentGateway.FraudApi.Features.Screening
{
    public static class Contracts
    {
        public record ScreeningRequest
        {
            public required string CardNumber { get; init; }

            public required string CardHolderName { get; init; }

            public required int ExpiryMonth { get; init; }

            public required int ExpiryYear { get; init; }
        }

        public record ScreeningResponse
        {
            public int SomeNumber { get; init; }
            public bool Accepted { get; init; }
        }
    }
}
