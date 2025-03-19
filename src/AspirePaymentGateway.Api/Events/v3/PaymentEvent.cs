using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Events.v3
{
    [DynamoDBTable(Constants.TableName)]
    [JsonDerivedType(typeof(PaymentRequestedEvent))]
    [JsonDerivedType(typeof(PaymentAuthorisedEvent))]
    [JsonDerivedType(typeof(PaymentDeclinedEvent))]
    public abstract record PaymentEvent(
        [property: DynamoDBHashKey]
        string Id,
        [property: DynamoDBRangeKey]
        string Action, 
        string OccurredAt)
    {
        // for dynamo serialization
        protected PaymentEvent() : this("", "", "")
        { }
    }
}
