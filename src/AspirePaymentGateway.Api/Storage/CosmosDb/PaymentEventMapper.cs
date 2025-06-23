using System.Text.Json;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using BinaryMash.Extensions.Results;
using OneOf;
using static AspirePaymentGateway.Api.Features.Payments.Domain.Errors;

namespace AspirePaymentGateway.Api.Storage.CosmosDb
{
    public class PaymentEventMapper(CosmosSystemTextJsonSerializer serializer)
    {
        private readonly string EventTypePropertyName = serializer.SerializeMemberName(typeof(PaymentEvent).GetProperty(nameof(PaymentEvent.EventType))!);

        public OneOf<PaymentEvent, ErrorDetail> MapToPaymentEvent(JsonElement doc)
        {
            string json = doc.ToString();
            var eventType = doc.GetProperty(EventTypePropertyName).GetString() ?? string.Empty;

            return eventType switch
            {
                nameof(PaymentRequestedEvent) => Deserialize<PaymentRequestedEvent>(json),
                nameof(PaymentScreenedEvent) => Deserialize<PaymentScreenedEvent>(json),
                nameof(PaymentAuthorisedEvent) => Deserialize<PaymentAuthorisedEvent>(json),
                nameof(PaymentDeclinedEvent) => Deserialize<PaymentDeclinedEvent>(json),
                _ => new UnknownEventTypeError(eventType)
            };
        }

        private PaymentEvent Deserialize<T>(string json) where T : PaymentEvent
        {
            return JsonSerializer.Deserialize<T>(json, serializer.JsonSerializerOptions)!;
        }
    }
}
