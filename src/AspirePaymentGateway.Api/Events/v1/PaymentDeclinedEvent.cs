using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v1
{
    public record PaymentDeclinedEvent : PaymentEvent
    {
        public required string Reason { get; init; }

        [SetsRequiredMembers]
        public PaymentDeclinedEvent(string paymentId, string occurredAt, string reason) 
            : base(paymentId, nameof(PaymentDeclinedEvent), occurredAt)
        {
            Reason = reason;   
        }

        // for dynamo serialization
        public PaymentDeclinedEvent()
        {
        }
    }
}
