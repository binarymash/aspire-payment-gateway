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
                OneOf<PaymentEvent, UnknownEventTypeError> map = MapFromDocument(document);
                //OneOf<PaymentEvent, UnknownEventTypeError> map = MapFromDocumentUsingReflection(document);

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
                    var map = MapToDocument(paymentEvent);
                    //var map = MapToDocumentUsingReflection(paymentEvent);

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

        private OneOf<PaymentEvent, UnknownEventTypeError> MapFromDocument(Document document)
        {
            //TODO: can we source-generate this?
            var eventType = document["EventType"].AsString();

            return eventType switch
            {
                nameof(PaymentRequestedEvent) => dynamoContext.FromDocument<PaymentRequestedEvent>(document),
                nameof(PaymentScreenedEvent) => dynamoContext.FromDocument<PaymentScreenedEvent>(document),
                nameof(PaymentAuthorisedEvent) => dynamoContext.FromDocument<PaymentAuthorisedEvent>(document),
                nameof(PaymentDeclinedEvent) => dynamoContext.FromDocument<PaymentDeclinedEvent>(document),
                _ => new UnknownEventTypeError(eventType)
            };
        }

        private OneOf<Document, UnknownEventTypeError> MapToDocument(PaymentEvent paymentEvent)
        {
            //TODO: can we source-generate this?
            return paymentEvent.EventType switch
            {
                nameof(PaymentRequestedEvent) => dynamoContext.ToDocument<PaymentRequestedEvent>(paymentEvent as PaymentRequestedEvent),
                nameof(PaymentScreenedEvent) => dynamoContext.ToDocument<PaymentScreenedEvent>(paymentEvent as PaymentScreenedEvent),
                nameof(PaymentAuthorisedEvent) => dynamoContext.ToDocument<PaymentAuthorisedEvent>(paymentEvent as PaymentAuthorisedEvent),
                nameof(PaymentDeclinedEvent) => dynamoContext.ToDocument<PaymentDeclinedEvent>(paymentEvent as PaymentDeclinedEvent),
                _ => new UnknownEventTypeError(paymentEvent.EventType)
            };
        }

        //private OneOf<PaymentEvent, UnknownEventTypeError> MapFromDocumentUsingReflection(Document document)
        //{
        //    //TODO: add caching for the reflection lookups, or this will be sloooooooooww

        //    var eventType = document["EventType"].AsString();

        //    var type = Type.GetType($"AspirePaymentGateway.Api.Features.Payments.Domain.Events.{eventType}");

        //    if (type is not null && typeof(PaymentEvent).IsAssignableFrom(type))
        //    {
        //        var method = typeof(IDynamoDBContext)
        //            .GetMethod("FromDocument", new[] { typeof(Document) })?
        //            .MakeGenericMethod(type);

        //        if (method is not null)
        //        {
        //            if (method.Invoke(dynamoContext, [document]) is PaymentEvent result)
        //            {
        //                return result;
        //            }
        //        }
        //    }

        //    return new UnknownEventTypeError(eventType);
        //}

        //public OneOf<Document, UnknownEventTypeError> MapToDocumentUsingReflection(PaymentEvent paymentEvent)
        //{
        //    //TODO: add caching for the reflection lookups, or this will be sloooooooooww

        //    var type = paymentEvent.GetType();

        //    var method = typeof(IDynamoDBContext)
        //        .GetMethods()
        //        .FirstOrDefault(m =>
        //            m.Name == "ToDocument" &&
        //            m.IsGenericMethod &&
        //            m.GetParameters().Length == 1)?
        //        .MakeGenericMethod(type);

        //    if (method is not null)
        //    {
        //        if (method.Invoke(dynamoContext, [paymentEvent]) is Document result)
        //        {
        //            return result;
        //        }
        //    }

        //    return new UnknownEventTypeError(paymentEvent.EventType);
        //}

        [LoggerMessage(Level = LogLevel.Information, Message = "Tried to save a payment with no changes")]
        partial void LogEmptySaveRequest();
    }
}
