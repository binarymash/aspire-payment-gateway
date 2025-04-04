namespace AspirePaymentGateway.Api.Features.Payments.Domain.Events
{
    public record PaymentRequestedEvent : PaymentEvent
    {
        public override string EventType { get; init; } = nameof(PaymentRequestedEvent);

        public required long Amount { get; init; }

        public required string Currency { get; init; }

        public required string CardNumber { get; init; }

        public required string CardHolderName { get; init; }

        public required int ExpiryMonth { get; init; }

        public required int ExpiryYear { get; init; }

        public required int Cvv { get; init; }
    }
}
