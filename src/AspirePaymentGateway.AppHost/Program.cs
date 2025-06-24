#if AWS
using AspirePaymentGateway.AppHost.Aws;
#elif AZURE
using AspirePaymentGateway.AppHost.Azure;
#endif

var builder = DistributedApplication.CreateBuilder(args);

#if AWS
(var awsConfig, var dynamoDb) = builder.AddAwsResources();
#elif AZURE
var paymentsCosmosContainer = builder.AddAzureResources();
#endif

var keycloakUser = builder.AddParameter("KeycloakAdminUsername");
var keycloakPassword = builder.AddParameter("KeycloakAdminPassword", secret: true);
var keycloak = builder.AddKeycloak("keycloak", port: 8080, adminUsername: keycloakUser, adminPassword: keycloakPassword)
    .WithDataVolume()
    .WithRealmImport("./keycloak/realms")
    .WithExternalHttpEndpoints();

var fraudApi = builder.AddProject<Projects.AspirePaymentGateway_FraudApi>("fraud-api")
    .WithReference(keycloak)
    .WaitFor(keycloak);

var mockBank = builder.AddProject<Projects.AspirePaymentGateway_MockBankApi>("mock-bank-api")
    .WithReference(keycloak)
    .WaitFor(keycloak);

builder.AddProject<Projects.AspirePaymentGateway_Api>("payment-gateway")
#if AWS
    .WithAwsDependencies(awsConfig, dynamoDb)
#elif AZURE
    .WithAzureDependencies(paymentsCosmosContainer)
#endif
    .WithReference(mockBank)
    .WithReference(fraudApi)
    .WithReference(keycloak)
    .WaitFor(keycloak);

await builder.Build().RunAsync();