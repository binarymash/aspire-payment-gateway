using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Events.v2
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
        public string Action { get; init; } = "Unknown";

        public required string OccurredAt { get; init; }
    }
}
