using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.DynamoDbAdmin
{
    public static class DynamoDbAdminResourceBuilderExtensions
    {
        private const string DynamoDbEndpointVariable = "DYNAMODB_ENDPOINT";
        private const string HostEnvironmentVariable = "HOST";
        private const string PortEnvironmentVariable = "PORT";

        public static IResourceBuilder<DynamoDbAdminResource> AddDynamoDbAdmin(
            this IDistributedApplicationBuilder builder,
            string name,
            string? host = null,
            string? port = null)
        {
            var resource = new DynamoDbAdminResource(name);

            EnvironmentCallbackContext ctx;

            return builder.AddResource(resource)
                .WithImage(DynamoDbAdminDefaults.Image)
                .WithImageRegistry(DynamoDbAdminDefaults.Registry)
                .WithImageTag(DynamoDbAdminDefaults.Tag)
                .WithEnvironment("DYNAMO_ENDPOINT", resource.GetEndpointFromString)
                .WithEnvironment(resource.GetEndpointFromEnvironmentCallbackContext)
                .WithHttpEndpoint(targetPort: 8001);

        }
    }

    internal static class DynamoDbAdminDefaults
    {
        internal const string Registry = "docker.io";

        internal const string Image = "aaronshaf/dynamodb-admin";

        internal const string Tag = "latest";

        internal const string DynamoEndpoint = "http://localhost:8000tyutyutyu";
    }
}
