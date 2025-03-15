namespace AspirePaymentGateway.Api.Events
{
    public record PaymentDeclinedEvent : PaymentEvent
    {
        public PaymentDeclinedEvent(string paymentId, DateTimeOffset occurredAt) : base(paymentId, nameof(PaymentDeclinedEvent))
        {
        }

        public PaymentDeclinedEvent()
        {
        }

        public required string Reason { get; init; }
    }
}
