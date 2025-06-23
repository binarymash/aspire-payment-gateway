using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using OneOf;
using static AspirePaymentGateway.Api.Features.Payments.Domain.Errors;

namespace AspirePaymentGateway.Api.Storage.DynamoDb
{
    public static class DocumentMapper
    {
        public static OneOf<Document, UnknownEventTypeError> MapToDocument(PaymentEvent paymentEvent, IDynamoDBContext dynamoContext)
        {
            //TODO: can we source-generate this?
            return paymentEvent.EventType switch
            {
                nameof(PaymentRequestedEvent) => dynamoContext.ToDocument((PaymentRequestedEvent)paymentEvent),
                nameof(PaymentScreenedEvent) => dynamoContext.ToDocument((PaymentScreenedEvent)paymentEvent),
                nameof(PaymentAuthorisedEvent) => dynamoContext.ToDocument((PaymentAuthorisedEvent)paymentEvent),
                nameof(PaymentDeclinedEvent) => dynamoContext.ToDocument((PaymentDeclinedEvent)paymentEvent),
                _ => new UnknownEventTypeError(paymentEvent.EventType)
            };
        }

        public static OneOf<PaymentEvent, UnknownEventTypeError> MapToPaymentEvent(Document document, IDynamoDBContext dynamoContext)
        {
            //TODO: can we source-generate this?
            var eventType = document["EventType"].AsString();

            return eventType switch
            {
                nameof(PaymentRequestedEvent) => dynamoContext.FromDocument<PaymentRequestedEvent>(document),
                nameof(PaymentScreenedEvent) => dynamoContext.FromDocument<PaymentScreenedEvent>(document),
                nameof(PaymentAuthorisedEvent) => dynamoContext.FromDocument<PaymentAuthorisedEvent>(document),
                nameof(PaymentDeclinedEvent) => dynamoContext.FromDocument<PaymentDeclinedEvent>(document),
                _ => new UnknownEventTypeError(eventType)
            };
        }
        public static OneOf<Document, UnknownEventTypeError> MapToDocumentUsingReflection(PaymentEvent paymentEvent, IDynamoDBContext dynamoContext)
        {
            //TODO: add caching for the reflection lookups, or this will be sloooooooooww

            var type = paymentEvent.GetType();

            var method = typeof(IDynamoDBContext)
                .GetMethods()
                .FirstOrDefault(m =>
                    m.Name == "ToDocument" &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 1)?
                .MakeGenericMethod(type);

            if (method is not null)
            {
                if (method.Invoke(dynamoContext, [paymentEvent]) is Document result)
                {
                    return result;
                }
            }

            return new UnknownEventTypeError(paymentEvent.EventType);
        }

        public static OneOf<PaymentEvent, UnknownEventTypeError> MapToPaymentEventUsingReflection(Document document, IDynamoDBContext dynamoContext)
        {
            //TODO: add caching for the reflection lookups, or this will be sloooooooooww

            var eventType = document["EventType"].AsString();

            var type = Type.GetType($"AspirePaymentGateway.Api.Features.Payments.Domain.Events.{eventType}");

            if (type is not null && typeof(PaymentEvent).IsAssignableFrom(type))
            {
                var method = typeof(IDynamoDBContext)
                    .GetMethod("FromDocument", [typeof(Document)])?
                    .MakeGenericMethod(type);

                if (method is not null)
                {
                    if (method.Invoke(dynamoContext, [document]) is PaymentEvent result)
                    {
                        return result;
                    }
                }
            }

            return new UnknownEventTypeError(eventType);
        }
    }
}
