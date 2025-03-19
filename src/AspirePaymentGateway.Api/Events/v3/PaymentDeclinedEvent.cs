using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v3
{
    public record PaymentDeclinedEvent(string Id, string OccurredAt, string Reason)
        : PaymentEvent(Id, nameof(PaymentDeclinedEvent), OccurredAt)
    {
        public PaymentDeclinedEvent() : this("", "", "")
        {
        }
    }
}
