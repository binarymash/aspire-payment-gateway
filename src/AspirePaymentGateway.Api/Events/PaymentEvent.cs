using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;

namespace AspirePaymentGateway.Api.Events
{
    //TODO: primary ctors
    //TODO: required fields
    [DynamoDBTable(Constants.TableName)]
    public abstract record PaymentEvent : IPaymentEvent
    {
        protected PaymentEvent(string id, string action)
        {
            Id = id;
            Action = action;
            OccurredAt = DateTimeOffset.UtcNow.ToString("O");
        }

        protected PaymentEvent()
        {
        }

        [DynamoDBHashKey]
        public string Id { get; init; }

        [DynamoDBRangeKey]
        public string Action { get; init; }

        public string OccurredAt { get; init; }
    }
}
