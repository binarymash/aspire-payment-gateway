using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using BinaryMash.Extensions.Results;
using OneOf;
using static AspirePaymentGateway.Api.Features.Payments.Domain.Errors;

namespace AspirePaymentGateway.Api.Storage.DynamoDb
{
    public partial class DynamoDbPaymentEventRepository(ILogger<DynamoDbPaymentEventRepository> logger, IDynamoDBContext dynamoContext, IAmazonDynamoDB dynamoClient) : IPaymentEventsRepository
    {
        public async Task<Result<IList<PaymentEvent>>> GetAsync(string paymentId, CancellationToken cancellationToken)
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
                return Result.Error<IList<PaymentEvent>>(new StorageReadExceptionError(ex));
            }

            List<PaymentEvent> events = [];

            foreach (var document in documents)
            {
                OneOf<PaymentEvent, UnknownEventTypeError> map = DocumentMapper.MapToPaymentEvent(document, dynamoContext);
                //OneOf<PaymentEvent, UnknownEventTypeError> map = DocumentMapper.MapToPaymentEventUsingReflection(document, dynamoContext);

                if (map.TryPickT0(out var @event, out var error))
                {
                    events.Add(@event);
                }
                else
                {
                    return Result.Error<IList<PaymentEvent>>(error);
                }
            }

            return events;
        }

        public async Task<Result> SaveAsync(IList<PaymentEvent> paymentEvents, CancellationToken cancellationToken)
        {
            try
            {
                if (paymentEvents.Count == 0)
                {
                    LogEmptySaveRequest();
                    return Result.Ok;
                }

                var transactWriteItemsRequest = new TransactWriteItemsRequest
                {
                    TransactItems = []
                };

                foreach (var paymentEvent in paymentEvents)
                {
                    var map = DocumentMapper.MapToDocument(paymentEvent, dynamoContext);
                    //var map = DocumentMapper.MapToDocumentUsingReflection(paymentEvent, dynamoContext);

                    if (map.TryPickT0(out var document, out var error))
                    {
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
                    else
                    {
                        return Result.Error<PaymentEvent>(error);
                    }
                }

                await dynamoClient.TransactWriteItemsAsync(transactWriteItemsRequest, cancellationToken);

                return Result.Ok;
            }
            catch (Exception ex)
            {
                return Result.Error<PaymentEvent>(new StorageWriteExceptionError(ex));
            }
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Tried to save a payment with no changes")]
        partial void LogEmptySaveRequest();
    }
}
