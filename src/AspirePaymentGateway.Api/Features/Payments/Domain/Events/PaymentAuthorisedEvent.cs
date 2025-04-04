using System.ComponentModel;

namespace AspirePaymentGateway.Api.Features.Payments.Domain.Events
{
    public record PaymentAuthorisedEvent : PaymentEvent
    {
        public override string EventType { get; init; } = nameof(PaymentAuthorisedEvent);
        
        [Description("Authorisation code from the bank")]
        public required string AuthorisationCode { get; init; }

    }
}
