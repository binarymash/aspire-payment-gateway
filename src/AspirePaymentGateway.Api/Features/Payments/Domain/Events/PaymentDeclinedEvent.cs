using System.ComponentModel;

namespace AspirePaymentGateway.Api.Features.Payments.Domain.Events
{
    public record PaymentDeclinedEvent : PaymentEvent
    {
        [Description("The reason for decline")]
        public required string Reason { get; init; }
        
        public override string EventType { get; init; } = nameof(PaymentDeclinedEvent);
    }
}
