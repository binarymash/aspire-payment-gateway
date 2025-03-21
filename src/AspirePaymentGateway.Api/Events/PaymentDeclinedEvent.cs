using System.ComponentModel;

namespace AspirePaymentGateway.Api.Events
{
    public record PaymentDeclinedEvent : PaymentEvent<PaymentDeclinedEvent>
    {
        [Description("The reason for decline")]
        public required string Reason { get; init; }
    }
}
