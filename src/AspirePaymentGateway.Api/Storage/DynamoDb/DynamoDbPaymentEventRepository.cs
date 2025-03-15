using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AspirePaymentGateway.Api.Events;
using OneOf;
using System.Collections.Generic;

namespace AspirePaymentGateway.Api.Storage.DynamoDb
{
    public class DynamoDbPaymentEventRepository : IPaymentEventRepository
    {
        private readonly IDynamoDBContext _dynamoContext;
        private readonly IAmazonDynamoDB _dynamoClient;

        //private static DynamoDBOperationConfig _operationConfig = new()
        //{
        //    OverrideTableName = Constants.TableName
        //};

        public DynamoDbPaymentEventRepository(IDynamoDBContext dynamoContext, IAmazonDynamoDB dynamoClient)
        {
            _dynamoContext = dynamoContext;
            _dynamoClient = dynamoClient;
        }

        public async Task<OneOf<IEnumerable<IPaymentEvent>, StorageError>> GetAsync(string paymentId, CancellationToken cancellationToken)
        {
            var events = new List<IPaymentEvent>();


            var request = new QueryRequest
            {
                TableName = Constants.TableName,
                KeyConditionExpression = "Id = :id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":id", new AttributeValue { S = paymentId } }
                }
            };

            var queryResult = await _dynamoClient.QueryAsync(request, cancellationToken);

            var documents = queryResult.Items.Select(item => Document.FromAttributeMap(item));

            foreach (var document in documents)
            {
                var action = document["Action"].AsString();

                PaymentEvent @event = action switch
                {
                    nameof(PaymentRequestedEvent) => _dynamoContext.FromDocument<PaymentRequestedEvent>(document),
                    nameof(PaymentAuthorisedEvent) => _dynamoContext.FromDocument<PaymentAuthorisedEvent>(document),
                    nameof(PaymentDeclinedEvent) => _dynamoContext.FromDocument<PaymentDeclinedEvent>(document),
                    _ => throw new InvalidOperationException($"Unknown item type: {action}")
                };

                events.Add(@event);
            }

            return events;
        }

        //public async Task<OneOf<IEnumerable<IPaymentEvent>, StorageError>> GetAsync(string paymentId, CancellationToken cancellationToken)
        //{
        //    var events = new List<IPaymentEvent>();

        //    var queryConfig = new QueryOperationConfig
        //    {
        //        KeyExpression = new Expression
        //        {
        //            ExpressionStatement = "Id = :id",
        //            ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
        //            {
        //                { ":id", paymentId }
        //            }
        //        },                
        //    };

        //    var queryResult = await _dynamo.QueryAsync<Document>(queryConfig, _operationConfig).GetRemainingAsync(cancellationToken);

        //    foreach (var document in queryResult)
        //    {
        //        var action = document["Action"].AsString();

        //        PaymentEvent @event = action switch
        //        {
        //            nameof(PaymentRequestedEvent) => _dynamo.FromDocument<PaymentRequestedEvent>(document),
        //            nameof(PaymentAuthorisedEvent) => _dynamo.FromDocument<PaymentAuthorisedEvent>(document),
        //            nameof(PaymentDeclinedEvent) => _dynamo.FromDocument<PaymentDeclinedEvent>(document),
        //            _ => throw new InvalidOperationException($"Unknown item type: {action}")
        //        };

        //        events.Add(@event);
        //    }

        //    return events;
        //}

        public async Task<OneOf<StorageOk, StorageError>> SaveAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent: IPaymentEvent
        {
            await _dynamoContext.SaveAsync(@event, cancellationToken);

            return new StorageOk();
        }

    }
}
