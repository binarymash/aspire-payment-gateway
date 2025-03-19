using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v4
{
    public record PaymentDeclinedEvent : PaymentEvent<PaymentDeclinedEvent>
    {
        public required string Reason { get; init; }
    }
}
