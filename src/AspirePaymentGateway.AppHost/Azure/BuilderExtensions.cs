namespace AspirePaymentGateway.AppHost.Azure
{
    internal static class BuilderExtensions
    {
        public static IResourceBuilder<Aspire.Hosting.Azure.AzureCosmosDBContainerResource> AddAzureResources(this IDistributedApplicationBuilder builder)
        {
#pragma warning disable ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var cosmosDb = builder.AddAzureCosmosDB("cosmos-db")
                .RunAsPreviewEmulator(emulator =>
                {
                    emulator.WithDataVolume();
                    emulator.WithDataExplorer();
                });
#pragma warning restore ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            var db = cosmosDb.AddCosmosDatabase("PaymentsDb");
            var paymentsTable = db.AddContainer("Payments", "/paymentId");

            return paymentsTable;
        }

        public static IResourceBuilder<ProjectResource> WithAzureDependencies(this IResourceBuilder<ProjectResource> resource, IResourceBuilder<Aspire.Hosting.Azure.AzureCosmosDBContainerResource> paymentsCosmosContainer)
        {
            resource.WithReference(paymentsCosmosContainer)
                .WaitFor(paymentsCosmosContainer);

            return resource;
        }
    }
}
