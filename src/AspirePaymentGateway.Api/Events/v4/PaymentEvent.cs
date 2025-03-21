using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Events.v4
{
    [DynamoDBTable(Constants.TableName)]
    public abstract record PaymentEvent<TPaymentEvent>: IPaymentEvent where TPaymentEvent : class
    {
        [DynamoDBHashKey]
        public required string Id { get; init; }

        [DynamoDBRangeKey]
        public string Action { get; private init; } = typeof(TPaymentEvent).Name;

        public required string OccurredAt { get; init; }
    }
}
