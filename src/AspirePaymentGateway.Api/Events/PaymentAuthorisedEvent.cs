using System.ComponentModel;

namespace AspirePaymentGateway.Api.Events
{
    public record PaymentAuthorisedEvent : PaymentEvent<PaymentAuthorisedEvent>
    {
        [Description("Authorisation code from the bank")]
        public required string AuthorisationCode { get; init; }

    }
}
