namespace AspirePaymentGateway.Api.Features.Payments.Domain.Events
{
    public record PaymentScreenedEvent : PaymentEvent
    {
        public override string EventType { get; init; } = nameof(PaymentScreenedEvent);

        public required bool ScreeningAccepted { get; init; }
    }
}
