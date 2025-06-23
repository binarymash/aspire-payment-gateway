using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
//using Amazon.DynamoDBv2;
//using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Features.Payments;
using AspirePaymentGateway.Api.Features.Payments.Services.BankApi;
using AspirePaymentGateway.Api.Features.Payments.Services.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using AspirePaymentGateway.Api.Features.Payments.Validation;
using AspirePaymentGateway.Api.Storage.CosmosDb;
using AspirePaymentGateway.Api.Telemetry;
using BinaryMash.Extensions.OAuth2.ClientCredentialsAuthorizationProvider;
using BinaryMash.Extensions.Redaction;
using FluentValidation;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Options;
using Refit;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;
using static Microsoft.Extensions.Hosting.Extensions;

namespace AspirePaymentGateway.Api
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds domain-specific service dependencies
        /// </summary>
        public static WebApplicationBuilder AddDomainServices(this WebApplicationBuilder builder)
        {
            //telemetry
            builder.Services
                .AddSingleton(ActivitySourceHelper.ActivitySource)
                .AddSingleton<BusinessMetrics>()
                    .AddOpenTelemetry().WithMetrics(metrics => metrics.AddMeter(BusinessMetrics.Name));

            // core extensions
            builder.Services
                .AddStandardDateTimeProvider()
                .AddRedaction(x => x.SetRedactor<StarRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData, DataTaxonomy.PiiData)));

            // domain
            builder.Services
                .AddSingleton<IValidator<PaymentRequest>, PaymentRequestValidator>()
                .AddSingleton<PaymentIdValidator>()
                .AddSingleton<PaymentSession>()
                .AddSingleton<CreatePaymentHandler>()
                .AddSingleton<GetPaymentHandler>();

            return builder;
        }

        public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
        {
            builder.Services
                .AddOpenApi(options =>
                    {
                        // add authentication to OpenAPI spec
                        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                    })
                .ConfigureHttpJsonOptions(options =>
                    {
                        // .NET pipeline does not play nicely with source-generated JsonSerializerContexts, so lets avoid it.
                        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                    })
                .AddProblemDetails(configure =>
                    {
                        // add Instance info to problem details
                        configure.CustomizeProblemDetails = (problemDetailsContext) => problemDetailsContext.ProblemDetails.Instance = $"{problemDetailsContext.HttpContext.Request.Method} {problemDetailsContext.HttpContext.Request.Path}";
                    })
                .AddAuthorization()
                .AddAuthentication()
                .AddKeycloakJwtBearer("keycloak", realm: "payment-gateway", options =>
                    {
                        options.RequireHttpsMetadata = false; //non-prod
                        options.Audience = "account";
                    });

            return builder;
        }

        /// <summary>
        /// Adds infrastructure-specific service dependencies
        /// </summary>
        public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            // core infrastructure

            //            builder.Services
            //                .AddAWSService<IAmazonDynamoDB>()
            //                .AddSingleton<IDynamoDBContext, DynamoDBContext>();


            // infrastructure

            // events repo

            var cosmosSerializer = new CosmosSystemTextJsonSerializer(CosmosSerializerOptions.Options);

            builder.AddAzureCosmosClient(
                    "Payments",
                    configureClientOptions: clientOptions => clientOptions.Serializer = cosmosSerializer);
            builder.Services
                .AddSingleton(cosmosSerializer)
                .AddSingleton<PaymentEventMapper>()
                .AddSingleton<IPaymentEventsRepository, CosmosDbPaymentEventRepository>();

            //builder.Services.AddSingleton<IPaymentEventsRepository, DynamoDbPaymentEventRepository>();
            //builder.Services.AddSingleton<IPaymentEventsRepository, InMemoryPaymentEventRepository>();

            // identity server

            builder.Services.AddHttpClient("IdentityServer", config => config.BaseAddress = new Uri(Constants.BaseUrls.IdentityServer));

            // fraud API

            builder.Services.AddSingleton<FraudApiTokenProvider>(sp =>
            {
                return new FraudApiTokenProvider(
                    httpClient: sp.GetRequiredService<IHttpClientFactory>().CreateClient("IdentityServer"),
                    options: Options.Create(builder.Configuration.GetSection("services:fraud-api:auth").Get<AuthorizationOptions>()!));
            });

            builder.Services.AddRefitClient<IFraudApi>(sp =>
            {
                return new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(FraudApiContractsContext.Default.Options),
                    AuthorizationHeaderValueGetter = sp.GetRequiredService<FraudApiTokenProvider>().GetClientCredentialAccessTokenAsync,
                };
            }).ConfigureHttpClient(c => c.BaseAddress = new Uri(Constants.BaseUrls.FraudApi));

            // bank API

            builder.Services.AddRefitClient<IBankApi>(sp =>
            {
                var tokenProvider = new BankApiTokenProvider(
                    httpClient: sp.GetRequiredService<IHttpClientFactory>().CreateClient("IdentityServer"),
                    options: Options.Create(builder.Configuration.GetSection("services:bank-api:auth").Get<AuthorizationOptions>()!));

                return new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(BankApiContractsContext.Default.Options),
                    AuthorizationHeaderValueGetter = tokenProvider.GetClientCredentialAccessTokenAsync,
                };
            }).ConfigureHttpClient(c => c.BaseAddress = new Uri(Constants.BaseUrls.BankApi));

            return builder;
        }
    }
}
