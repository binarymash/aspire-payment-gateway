using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v4
{
    public record PaymentAuthorisedEvent : PaymentEvent<PaymentAuthorisedEvent>
    {
        public required string AuthorisationCode { get; init; }

    }
}
