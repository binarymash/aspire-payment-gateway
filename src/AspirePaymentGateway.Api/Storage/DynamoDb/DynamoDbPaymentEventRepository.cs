using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AspirePaymentGateway.Api.Extensions.Results;
using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using OneOf;
using static AspirePaymentGateway.Api.Features.Payments.Domain.Errors;

namespace AspirePaymentGateway.Api.Storage.DynamoDb
{
    public partial class DynamoDbPaymentEventRepository(ILogger<DynamoDbPaymentEventRepository> logger, IDynamoDBContext dynamoContext, IAmazonDynamoDB dynamoClient) : IPaymentEventsRepository
    {
        //TODO: fix this

        public async Task<Result<IList<IPaymentEvent>>> GetAsync(string paymentId, CancellationToken cancellationToken)
        {
            List<Document> documents;

            try
            {
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

                documents = queryResult.Items.ConvertAll(item => Document.FromAttributeMap(item));
            }
            catch (Exception ex)
            {
                return Result.Error<IList<IPaymentEvent>>(new StorageReadExceptionError(ex));
            }

            List<IPaymentEvent> events = [];

            foreach (var document in documents)
            {
                var eventType = document["EventType"].AsString();

                OneOf<PaymentEvent, UnknownEventTypeError> map = eventType switch
                {
                    nameof(PaymentRequestedEvent) => dynamoContext.FromDocument<PaymentRequestedEvent>(document),
                    nameof(PaymentAuthorisedEvent) => dynamoContext.FromDocument<PaymentAuthorisedEvent>(document),
                    nameof(PaymentDeclinedEvent) => dynamoContext.FromDocument<PaymentDeclinedEvent>(document),
                    _ => new UnknownEventTypeError(eventType)
                };

                if (map.TryPickT0(out var @event, out var error))
                {
                    events.Add(@event);
                }
                else
                {
                    return Result.Error<IList<IPaymentEvent>>(error);
                }
            }

            return events;
        }

        public async Task<Result> SaveAsync(IList<IPaymentEvent> paymentEvents, CancellationToken cancellationToken)
        {
            try
            {
                ///BatchWrite<PaymentEvent> batch = dynamoContext.CreateBatchWrite<PaymentEvent>();
                ///foreach (var @event in payment.UncommittedEvents)
                ///{
                ///    batch.AddPutItems(payment.UncommittedEvents);
                ///}
                ///await batch.ExecuteAsync(cancellationToken);
                if (paymentEvents.Count == 0)
                {
                    LogEmptySaveRequest();
                    return Result.Ok;
                }

                var transactWriteItemsRequest = new TransactWriteItemsRequest
                {
                    TransactItems = []
                };

                foreach (var @event in paymentEvents)
                {
                    var document = dynamoContext.ToDocument<IPaymentEvent>(@event);
                    var putRequest = new Put
                    {
                        TableName = Constants.TableName,
                        Item = document.ToAttributeMap()
                    };

                    transactWriteItemsRequest.TransactItems.Add(new TransactWriteItem
                    {
                        Put = putRequest
                    });
                }

                await dynamoClient.TransactWriteItemsAsync(transactWriteItemsRequest, cancellationToken);

                return Result.Ok;
            }
            catch (Exception ex)
            {
                return Result.Error<Payment>(new StorageWriteExceptionError(ex));
            }
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Tried to save a payment with no changes")]
        partial void LogEmptySaveRequest();
    }
}
