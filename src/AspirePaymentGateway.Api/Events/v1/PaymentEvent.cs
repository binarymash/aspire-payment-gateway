using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Events.v1
{
    [DynamoDBTable(Constants.TableName)]
    [JsonDerivedType(typeof(PaymentRequestedEvent))]
    [JsonDerivedType(typeof(PaymentAuthorisedEvent))]
    [JsonDerivedType(typeof(PaymentDeclinedEvent))]
    public abstract record PaymentEvent //: IPaymentEvent
    {
        [DynamoDBHashKey]
        public required string Id { get; init; }

        [DynamoDBRangeKey]
        public required string Action { get; init; }

        public required string OccurredAt { get; init; }

        [SetsRequiredMembers]
        protected PaymentEvent(string paymentId, string action, string occurredAt)
        {
            Id = paymentId;
            Action = action;
            OccurredAt = occurredAt;
        }

        // for dynamo serialization
        protected PaymentEvent() { }
    }
}
