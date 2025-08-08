using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Aspire.Hosting.AWS;

namespace AspirePaymentGateway.AppHost.Aws
{
    internal static class BuilderExtensions
    {
        public static (IAWSSDKConfig, IResourceBuilder<ContainerResource>) AddAwsResources(this IDistributedApplicationBuilder builder)
        {
            var awsConfig = builder.AddAWSSDKConfig().WithProfile("default");

            IResourceBuilder<ContainerResource> dynamoDb = builder.AddContainer("dynamodb", "instructure/dynamo-local-admin")
                .WithHttpEndpoint(targetPort: 8000)
                .OnResourceReady(async (db, evt, ct) => await CreatePaymentsTableAsync(db, ct));

            return (awsConfig, dynamoDb);
        }

        public static IResourceBuilder<ProjectResource> WithAwsDependencies(this IResourceBuilder<ProjectResource> resource, IAWSSDKConfig awsConfig, IResourceBuilder<ContainerResource> dynamoDb)
        {
            resource.WithReference(awsConfig)
                .WaitFor(dynamoDb)
                .WithEnvironment("AWS_ENDPOINT_URL_DYNAMODB", dynamoDb.Resource.GetEndpoint("http"));

            return resource;
        }

        private static async Task CreatePaymentsTableAsync(ContainerResource db, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var ddbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = db.GetEndpoint("http").Url });

            // Create the Accounts table.
            await ddbClient.CreateTableAsync(new CreateTableRequest
            {
                TableName = "Payments",
                AttributeDefinitions =
                [
                    new() { AttributeName = "PaymentId", AttributeType = ScalarAttributeType.S },
                    new() { AttributeName = "OccurredAt", AttributeType = ScalarAttributeType.S },
                ],
                KeySchema =
                [
                    new() { AttributeName = "PaymentId", KeyType = KeyType.HASH },
                    new() { AttributeName = "OccurredAt", KeyType = KeyType.RANGE }
                ],
                BillingMode = Amazon.DynamoDBv2.BillingMode.PAY_PER_REQUEST
            }, cancellationToken);
        }
    }
}