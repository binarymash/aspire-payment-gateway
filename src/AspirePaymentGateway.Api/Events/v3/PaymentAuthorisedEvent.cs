using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v3
{
    public record PaymentAuthorisedEvent(string Id, string OccurredAt, string AuthorisationCode) 
        : PaymentEvent(Id, nameof(PaymentAuthorisedEvent), OccurredAt)
    {                 
        public PaymentAuthorisedEvent() : this("", "", "")
        {
        }
    }
}
