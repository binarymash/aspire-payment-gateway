using System.ComponentModel;

namespace AspirePaymentGateway.Api.Features.Payments.Events
{
    public record PaymentDeclinedEvent : PaymentEvent<PaymentDeclinedEvent>
    {
        [Description("The reason for decline")]
        public required string Reason { get; init; }
    }
}
