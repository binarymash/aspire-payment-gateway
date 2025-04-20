using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig().WithProfile("default");

IResourceBuilder<ContainerResource> dynamoDb = builder.AddContainer("dynamodb", "instructure/dynamo-local-admin")
    .WithHttpEndpoint(targetPort: 8000);

builder.Eventing.Subscribe<ResourceReadyEvent>(dynamoDb.Resource, async (_, cancellationToken) =>
{
    var endpointUrl = dynamoDb.Resource.GetEndpoint("http").Url;
    await CreatePaymentsTableAsync(endpointUrl, cancellationToken);
});

var keycloakUser = builder.AddParameter("KeycloakAdminUsername");
var keycloakPassword = builder.AddParameter("KeycloakAdminPassword", secret: true);

var keycloak = builder.AddKeycloak("keycloak", port: 8080, adminUsername: keycloakUser, adminPassword: keycloakPassword)
    .WithDataVolume()
    .WithRealmImport("./keycloak/realms")
    .WithExternalHttpEndpoints()
    .WithLifetime(ContainerLifetime.Persistent);

var fraudApi = builder.AddProject<Projects.AspirePaymentGateway_FraudApi>("fraud-api");

var mockBank = builder.AddProject<Projects.AspirePaymentGateway_MockBankApi>("mock-bank-api");

builder.AddProject<Projects.AspirePaymentGateway_Api>("payment-gateway")
    .WithReference(awsConfig)
    .WithReference(mockBank)
    .WithReference(fraudApi)
    .WaitFor(dynamoDb)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithEnvironment("AWS_ENDPOINT_URL_DYNAMODB", dynamoDb.Resource.GetEndpoint("http"));

await builder.Build().RunAsync();

static async Task CreatePaymentsTableAsync(string serviceUrl, CancellationToken cancellationToken)
{
    var ddbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = serviceUrl });

    // Create the Accounts table.
    await ddbClient.CreateTableAsync(new CreateTableRequest
    {
        TableName = "Payments",
        AttributeDefinitions =
        [
            new() { AttributeName = "Id", AttributeType = ScalarAttributeType.S },
            new() { AttributeName = "OccurredAt", AttributeType = ScalarAttributeType.S }
        ],
        KeySchema =
        [
            new() { AttributeName = "Id", KeyType = KeyType.HASH },
            new() {AttributeName = "OccurredAt", KeyType = KeyType.RANGE}
        ],
        BillingMode = Amazon.DynamoDBv2.BillingMode.PAY_PER_REQUEST
    }, cancellationToken);
}
