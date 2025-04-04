using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Features.Payments.Domain.Events
{
    //Required for serialization in api responses
    [JsonDerivedType(typeof(PaymentRequestedEvent))]
    [JsonDerivedType(typeof(PaymentAuthorisedEvent))]
    [JsonDerivedType(typeof(PaymentDeclinedEvent))]
    public interface IPaymentEvent
    {
        public string Id { get; }

        public string OccurredAt { get; }

        public string EventId { get; }

        public string EventType { get; }
    }
}