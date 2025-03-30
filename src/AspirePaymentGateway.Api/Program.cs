using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Extensions.Http.Logging;
using AspirePaymentGateway.Api.Extensions.Redaction;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.BankApi;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.EventStore;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.CreatePayment.Validation;
using AspirePaymentGateway.Api.Features.Payments.GetPayment;
using AspirePaymentGateway.Api.Features.Payments.GetPayment.EventStore;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using AspirePaymentGateway.Api.Telemetry;
using FluentValidation;
using Microsoft.Extensions.Compliance.Classification;
using Refit;
using System.Text.Json;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.GetPayment.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//domain-specific metrics
builder.Services.AddSingleton<BusinessMetrics>();
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddMeter(BusinessMetrics.Name));

// infrastructure
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

// fraud API
var fraudApiClientBuilder = builder.Services.AddRefitClient<IFraudApi>(new RefitSettings(new SystemTextJsonContentSerializer(FraudApiContractsContext.Default.Options)))
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fraud-api"))
    .AddHttpMessageHandler<LoggingDelegatingHandler>();

// bank API
builder.Services.AddRefitClient<IBankApi>(new RefitSettings(new SystemTextJsonContentSerializer(BankApiContractsContext.Default.Options)))
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://mock-bank-api"))
    .AddHttpMessageHandler<LoggingDelegatingHandler>();

// domain classes
builder.Services.AddSingleton<IValidator<PaymentRequest>, PaymentRequestValidator>();
builder.Services.AddSingleton<ISavePaymentEvent, DynamoDbPaymentEventRepository>();
builder.Services.AddSingleton<IGetPaymentEvent, DynamoDbPaymentEventRepository>();
builder.Services.AddStandardDateTimeProvider();
builder.Services.AddSingleton<CreatePaymentHandler>();
builder.Services.AddSingleton<GetPaymentHandler>();
builder.Services.AddTransient<LoggingDelegatingHandler>();

// redaction
builder.Services.AddRedaction(x =>
{
    x.SetRedactor<StarRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData, DataTaxonomy.PiiData));
});

// endpoint serialization configuration
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // .NET pipeline does not play nicely with source-generated JsonSerializerContexts, so lets avoid it.
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

// add Instance info to problem details
builder.Services.AddProblemDetails(configure =>
{
    configure.CustomizeProblemDetails = (problemDetailsContext) =>
    {
        problemDetailsContext.ProblemDetails.Instance = $"{problemDetailsContext.HttpContext.Request.Method} {problemDetailsContext.HttpContext.Request.Path}";
    };
});

var app = builder.Build();

app.UseFluentValidationNamingFromJsonOptions();
app.MapDefaultEndpoints();
app.MapOpenApiForDevelopment("/scalar/v1");
app.UseHttpsRedirection();

// Map endpoints

app.MapPost("/payments",
        async (CreatePaymentHandler handler, PaymentRequest request, CancellationToken cancellationToken) => await handler.PostPaymentAsync(request, cancellationToken))
    .WithSummary("Make Payment")
    .WithDescription("Makes a payment on the specified card: Fraud screening is performed before the request is sent to the bank for authorisation")
    .Produces<PaymentResponse>(StatusCodes.Status201Created)
    .ProducesValidationProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapGet("/payments/{paymentId}",
        async (GetPaymentHandler handler, string paymentId, CancellationToken cancellationToken) => await handler.GetPaymentAsync(paymentId, cancellationToken))
    .WithSummary("Get Payment")
    .WithDescription("Retrieves the payment events for the specified payment")
    .Produces<GetPaymentResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();
