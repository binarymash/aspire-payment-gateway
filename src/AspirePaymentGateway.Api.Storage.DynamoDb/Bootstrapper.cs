using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;

namespace AspirePaymentGateway.Api.Storage.DynamoDb
{
    public class Bootstrapper
    {
        public async Task CreatePaymentsTableAsync(string serviceUrl, CancellationToken cancellationToken)
        {
            var ddbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = serviceUrl });

            // Create the Accounts table.
            await ddbClient.CreateTableAsync(new CreateTableRequest
            {
                TableName = Constants.TableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "Id", AttributeType = "S" }
                },
                        KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = "Id", KeyType = "HASH" }
                },
                BillingMode = Amazon.DynamoDBv2.BillingMode.PAY_PER_REQUEST
            }, cancellationToken);

            // Add an account to the Accounts table.
            //    await ddbClient.PutItemAsync(new PutItemRequest
            //    {
            //        TableName = "Accounts",
            //        Item = new Dictionary<string, AttributeValue>
            //{
            //    { "Id", new AttributeValue("1") },
            //    { "Name", new AttributeValue("Amazon") },
            //    { "Address", new AttributeValue("Seattle, WA") }
            //}
            //    });
        }
    }
}
