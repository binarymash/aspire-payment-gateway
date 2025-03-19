using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v2
{
    public record PaymentDeclinedEvent : PaymentEvent
    {
        public PaymentDeclinedEvent()
        {
            Action = nameof(PaymentDeclinedEvent);
        }

        public required string Reason { get; init; }
    }
}
