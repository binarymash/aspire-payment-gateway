//using Aspire.Hosting.ApplicationModel;

//namespace Aspire.Hosting.DynamoDbAdmin
//{
//    public static class DynamoDbAdmin2ResourceBuilderExtensions
//    {
//        private const string DynamoDbEndpointVariable = "DYNAMODB_ENDPOINT";
//        private const string HostEnvironmentVariable = "HOST";
//        private const string PortEnvironmentVariable = "PORT";

//        public static IResourceBuilder<DynamoDbAdminResource> AddDynamoDbAdmin(
//            this IDistributedApplicationBuilder builder,
//            string name,
//            string? host = null,
//            string? port = null)
//        {
//            var resource = new DynamoDbAdminResource(name);

//            EnvironmentCallbackContext ctx;

//            return builder.AddResource(resource)
//                .WithImage(DynamoDbAdminDefaults.Image)
//                .WithImageRegistry(DynamoDbAdminDefaults.Registry)
//                .WithImageTag(DynamoDbAdminDefaults.Tag)
//                .WithEnvironment("DYNAMO_ENDPOINT", resource.GetEndpointFromString)
//                .WithEnvironment(resource.GetEndpointFromEnvironmentCallbackContext)
//                .WithHttpEndpoint(targetPort: 8001);

//        }

//        public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IResourceBuilder<DynamoDbAdmin2Resource> source)
//            where TDestination : IResourceWithEnvironment
//        {
//            ArgumentNullException.ThrowIfNull(builder);
//            ArgumentNullException.ThrowIfNull(source);

//            builder.WithAnnotation()
//            return builder;
//        }
//    }

//    internal static class DynamoDbAdminDefaults
//    {
//        internal const string Registry = "docker.io";

//        internal const string Image = "instructure/dynamo-local-admin";

//        internal const string Tag = "latest";
//    }
//}
