using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Extensions.Http.Auth;
using AspirePaymentGateway.Api.Extensions.Redaction;
using AspirePaymentGateway.Api.Features.Payments;
using AspirePaymentGateway.Api.Features.Payments.Services.BankApi;
using AspirePaymentGateway.Api.Features.Payments.Services.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using AspirePaymentGateway.Api.Features.Payments.Validation;
using AspirePaymentGateway.Api.Storage.InMemory;
using AspirePaymentGateway.Api.Telemetry;
using FluentValidation;
using Microsoft.Extensions.Compliance.Classification;
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
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            //telemetry
            services
                .AddSingleton(ActivitySourceHelper.ActivitySource)
                .AddSingleton<BusinessMetrics>()
                    .AddOpenTelemetry().WithMetrics(metrics => metrics.AddMeter(BusinessMetrics.Name));

            // core extensions
            services
                .AddStandardDateTimeProvider()
                .AddRedaction(x => x.SetRedactor<StarRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData, DataTaxonomy.PiiData)));

            // domain
            services
                .AddSingleton<IValidator<PaymentRequest>, PaymentRequestValidator>()
                .AddSingleton<PaymentIdValidator>()
                .AddSingleton<PaymentSession>()
                .AddSingleton<CreatePaymentHandler>()
                .AddSingleton<GetPaymentHandler>()
                .AddTransient<AuthDelegatingHandler>();

            return services;
        }

        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services
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

            return services;
        }

        /// <summary>
        /// Adds infrastructure-specific service dependencies
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // core infrastructure
            services
                .AddAWSService<IAmazonDynamoDB>()
                .AddSingleton<IDynamoDBContext, DynamoDBContext>();

            // infrastructure

            // events repo
            //TODO: fix AddSingleton<IPaymentEventsRepository, DynamoDbPaymentEventRepository>();
            services.AddSingleton<IPaymentEventsRepository, InMemoryPaymentEventRepository>();

            // fraud API
            services.AddRefitClient<IFraudApi>(new RefitSettings(new SystemTextJsonContentSerializer(FraudApiContractsContext.Default.Options)))
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fraud-api"))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            // bank API
            services.AddRefitClient<IBankApi>(new RefitSettings(new SystemTextJsonContentSerializer(BankApiContractsContext.Default.Options)))
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://mock-bank-api"))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            return services;
        }
    }
}
