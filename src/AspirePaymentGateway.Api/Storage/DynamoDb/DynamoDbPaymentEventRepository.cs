using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.EventStore;
using AspirePaymentGateway.Api.Features.Payments.Events;
using AspirePaymentGateway.Api.Features.Payments.GetPayment.EventStore;
using OneOf;

namespace AspirePaymentGateway.Api.Storage.DynamoDb
{
    public class DynamoDbPaymentEventRepository(IDynamoDBContext dynamoContext, IAmazonDynamoDB dynamoClient) : IGetPaymentEvent, ISavePaymentEvent
    {
        public async Task<OneOf<IEnumerable<IPaymentEvent>, Exception>> GetAsync(string paymentId, CancellationToken cancellationToken)
        {
            try
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

                var queryResult = await dynamoClient.QueryAsync(request, cancellationToken);

                var documents = queryResult.Items.Select(item => Document.FromAttributeMap(item));

                foreach (var document in documents)
                {
                    var action = document["EventType"].AsString();

                    OneOf<IPaymentEvent, Exception> map = action switch
                    {
                        nameof(PaymentRequestedEvent) => dynamoContext.FromDocument<PaymentRequestedEvent>(document),
                        nameof(PaymentAuthorisedEvent) => dynamoContext.FromDocument<PaymentAuthorisedEvent>(document),
                        nameof(PaymentDeclinedEvent) => dynamoContext.FromDocument<PaymentDeclinedEvent>(document),
                        _ => new InvalidOperationException($"Unknown event type: {action}")
                    };

                    if (map.TryPickT0(out var @event, out var exception))
                    {
                        events.Add(@event);
                    }
                    else
                    {
                        return exception;
                    }
                }

                return events;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public async Task<OneOf<StorageOk, Exception>> SaveAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IPaymentEvent
        {
            try
            {
                await dynamoContext.SaveAsync(@event, cancellationToken);

                return new StorageOk();
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

    }
}
