using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using OneOf;

namespace AspirePaymentGateway.Api.Storage.DynamoDb
{
    public class DynamoDbPaymentRepository : IPaymentRepository
    {
        private readonly IAmazonDynamoDB _dynamo;

        public DynamoDbPaymentRepository(IAmazonDynamoDB dynamo)
        {
            _dynamo = dynamo;
        }

        //public async Task<OneOf<IEnumerable<PaymentEvent>, StorageError>> GetPaymentEventsAsync(string paymentId, CancellationToken cancellationToken)
        //{
        //    var events = new List<PaymentEvent>();

        //    QueryRequest request = new()
        //    {
        //        TableName = Constants.TableName,
        //        KeyConditionExpression = "Id = :id",
        //        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        //        {
        //            { ":id", new AttributeValue { S = paymentId } }
        //        }
        //    };


        //    var query = await _dynamo.QueryAsync(request, cancellationToken);

        //    foreach (var item in query.Items)
        //    {
        //        var action = item["Action"].ToString();

        //        PaymentEvent @event = action switch
        //        {
        //            nameof(PaymentRequestedEvent) => _dynamo.FromDocument<PaymentRequestedEvent>(item),
        //            nameof(PaymentAuthorisedEvent) => _dynamo.FromDocument<PaymentAuthorisedEvent>(item),
        //            nameof(PaymentDeclinedEvent) => _dynamo.FromDocument<PaymentDeclinedEvent>(item),
        //            _ => throw new InvalidOperationException($"Unknown item type: {action}")
        //        };
        //        events.Add(@event);
        //    }

        //    return events;
        //}

        public async Task<OneOf<StorageOk, StorageError>> SavePaymentRequestAsync(string paymentId, PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            PutItemRequest request = new()
            {
                TableName = Constants.TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = paymentId } },
                    { "Action", new AttributeValue { S = "REQUEST" } },
                    { "Amount", new AttributeValue { N = paymentRequest.Payment.Amount.ToString() } },
                    { "Currency", new AttributeValue { S = paymentRequest.Payment.CurrencyCode } },
                    { "CardNumber", new AttributeValue { S = paymentRequest.Card.CardNumber } },
                    { "ExpiryMonth", new AttributeValue { N = paymentRequest.Card.Expiry.Month.ToString() } },
                    { "ExpiryYear", new AttributeValue { N = paymentRequest.Card.Expiry.Year.ToString() } },
                    { "Cvv", new AttributeValue { S = paymentRequest.Card.CVV.ToString() } },
                    { "CreatedDate", new AttributeValue { S = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") } }
                }
            };

            var result = await _dynamo.PutItemAsync(request, cancellationToken);
            if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return new StorageError();
            }

            return new StorageOk();
        }

        public async Task<OneOf<StorageOk, StorageError>> SavePaymentResultAsync(string paymentId, string paymentStatus, CancellationToken cancellationToken)
        {
            PutItemRequest request = new()
            {
                TableName = Constants.TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = paymentId } },
                    { "Action", new AttributeValue { S = paymentStatus } },
                    { "CreatedDate", new AttributeValue { S = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") } }
                }
            };

            var result = await _dynamo.PutItemAsync(request, cancellationToken);
            if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return new StorageError();
            }

            return new StorageOk();
        }
    }
}
