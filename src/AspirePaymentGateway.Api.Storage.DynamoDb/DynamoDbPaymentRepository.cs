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

        public async Task<OneOf<PaymentRequest, StorageError>> SavePaymentRequestAsync(string paymentId, PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            PutItemRequest request = new()
            {
                TableName = Constants.TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "PaymentId", new AttributeValue { S = paymentId } },
                    { "Amount", new AttributeValue { N = paymentRequest.Amount.ToString() } },
                    { "Currency", new AttributeValue { S = paymentRequest.CurrencyCode } },
                    { "CardNumber", new AttributeValue { S = paymentRequest.CardNumber } },
                    { "ExpiryMonth", new AttributeValue { N = paymentRequest.ExpiryMonth.ToString() } },
                    { "ExpiryYear", new AttributeValue { N = paymentRequest.ExpiryYear.ToString() } },
                    { "Cvv", new AttributeValue { S = paymentRequest.CVV.ToString() } },
                    { "CreatedDate", new AttributeValue { S = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") } }
                }
            };

            var result = await _dynamo.PutItemAsync(request, cancellationToken);
            if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return new StorageError();
            }

            return paymentRequest;


        }

    }
}
