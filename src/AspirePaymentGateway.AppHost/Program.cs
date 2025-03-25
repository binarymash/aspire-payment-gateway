using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig().WithProfile("default");

IResourceBuilder<ContainerResource> dynamoDb = builder.AddContainer("dynamodb", "instructure/dynamo-local-admin")
    .WithHttpEndpoint(targetPort: 8000);

builder.Eventing.Subscribe<ResourceReadyEvent>(dynamoDb.Resource, async (@event, cancellationToken) =>
{
    var endpointUrl = dynamoDb.Resource.GetEndpoint("http").Url;
    await CreatePaymentsTableAsync(endpointUrl, cancellationToken);
});

var fraudApi = builder.AddProject<Projects.AspirePaymentGateway_FraudApi>("fraud-api");

var mockBank = builder.AddProject<Projects.AspirePaymentGateway_MockBankApi>("mock-bank-api");

var paymentGateway = builder.AddProject<Projects.AspirePaymentGateway_Api>("payment-gateway")
    .WithReference(awsConfig)
    .WithReference(mockBank)
    .WithReference(fraudApi)
    .WaitFor(dynamoDb)
    .WithEnvironment("AWS_ENDPOINT_URL_DYNAMODB", dynamoDb.Resource.GetEndpoint("http"));


builder.Build().Run();

async Task CreatePaymentsTableAsync(string serviceUrl, CancellationToken cancellationToken)
{
    try
    {
        var ddbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = serviceUrl });

        // Create the Accounts table.
        var result = await ddbClient.CreateTableAsync(new CreateTableRequest
        {
            TableName = "Payments",
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition { AttributeName = "Id", AttributeType = ScalarAttributeType.S },
                new AttributeDefinition { AttributeName = "OccurredAt", AttributeType = ScalarAttributeType.S }
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement { AttributeName = "Id", KeyType = KeyType.HASH },
                new KeySchemaElement {AttributeName = "OccurredAt", KeyType = KeyType.RANGE}
            },
            BillingMode = Amazon.DynamoDBv2.BillingMode.PAY_PER_REQUEST
        }, cancellationToken);
    }
    catch (Exception ex)
    {

    }
}
