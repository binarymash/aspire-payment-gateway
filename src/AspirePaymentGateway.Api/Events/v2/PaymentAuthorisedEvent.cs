using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v2
{
    public record PaymentAuthorisedEvent : PaymentEvent
    {
        public PaymentAuthorisedEvent()
        {
            Action = nameof(PaymentAuthorisedEvent);
        }

        public required string AuthorisationCode { get; init; }

    }
}
