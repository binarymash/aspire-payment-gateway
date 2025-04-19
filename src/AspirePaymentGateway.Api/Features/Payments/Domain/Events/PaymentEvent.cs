using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;

namespace AspirePaymentGateway.Api.Features.Payments.Domain.Events
{
    [DynamoDBTable(Constants.TableName)]
    public abstract record PaymentEvent : IPaymentEvent
    {
        [DynamoDBHashKey]
        [Description("The Payment ID")]
        public required string Id { get; init; }

        [DynamoDBRangeKey]
        [Description("When the event occurred")]
        public required string OccurredAt { get; init; }

        [Description("The event type")]
        public abstract string EventType { get; init; }

        [Description("The event ID")]
        public string EventId { get; private init; } = $"evt_{Guid.NewGuid().ToString()}";

    }
}
