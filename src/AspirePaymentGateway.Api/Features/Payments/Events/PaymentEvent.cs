using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using System.ComponentModel;

namespace AspirePaymentGateway.Api.Features.Payments.Events
{
    [DynamoDBTable(Constants.TableName)]
    public abstract record PaymentEvent<TPaymentEvent> : IPaymentEvent where TPaymentEvent : class
    {
        [DynamoDBHashKey]
        [Description("The Payment ID")]
        public required string Id { get; init; }

        [DynamoDBRangeKey]
        [Description("When the event occurred")]
        public required string OccurredAt { get; init; }

        [Description("The event ID")]
        public string EventId { get; private init; } = $"evt_{Guid.NewGuid().ToString()}";

        [Description("The event type")]
        public string EventType { get; private init; } = typeof(TPaymentEvent).Name;
    }
}
