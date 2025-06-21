using System.ComponentModel;

namespace AspirePaymentGateway.Api.Features.Payments.Domain.Events
{
    //[DynamoDBTable(tableName: Storage.DynamoDb.Constants.TableName)]
    public record PaymentEvent
    {
        [Description("The Payment ID")]
        public required string Id { get; init; }

        [Description("When the event occurred")]
        public required string OccurredAt { get; init; }

        [Description("The event type")]
        public virtual string EventType { get; init; } = string.Empty;

        [Description("The event ID")]
        public string EventId { get; private init; } = $"evt_{Guid.NewGuid()}";
    }
}
