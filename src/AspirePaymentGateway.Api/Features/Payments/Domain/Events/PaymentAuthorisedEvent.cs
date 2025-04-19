using System.ComponentModel;

namespace AspirePaymentGateway.Api.Features.Payments.Domain.Events
{
    public record PaymentAuthorisedEvent : PaymentEvent
    {
        public override string EventType { get; init; } = nameof(PaymentAuthorisedEvent);

        [Description("Authorisation request trace ID")]
        public required string AuthorisationTraceId { get; set; }

        [Description("Is the payment authorised?")]
        public required bool IsAuthorised { get; set; }

        [Description("Authorisation code from the bank")]
        public string? AuthorisationCode { get; init; }
    }
}
