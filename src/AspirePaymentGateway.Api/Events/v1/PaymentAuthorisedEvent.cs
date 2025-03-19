using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v1
{
    public record PaymentAuthorisedEvent : PaymentEvent
    {
        public required string AuthorisationCode { get; init; }

        [SetsRequiredMembers]
        public PaymentAuthorisedEvent(string paymentId, string occurredAt, string authorisationCode) : base(paymentId, nameof(PaymentAuthorisedEvent), occurredAt)
        {
            AuthorisationCode = authorisationCode;            
        }

        // for dynamo serialization
        public PaymentAuthorisedEvent()
        {
        }
    }
}
