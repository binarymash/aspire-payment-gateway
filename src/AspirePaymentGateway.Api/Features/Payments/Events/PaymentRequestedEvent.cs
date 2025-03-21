namespace AspirePaymentGateway.Api.Features.Payments.Events
{
    public record PaymentRequestedEvent : PaymentEvent<PaymentRequestedEvent>
    {
        public required long Amount { get; init; }

        public required string Currency { get; init; }

        public required string CardNumber { get; init; }

        public required string CardHolderName { get; init; }

        public required int ExpiryMonth { get; init; }

        public required int ExpiryYear { get; init; }

        public required int Cvv { get; init; }
    }
}
