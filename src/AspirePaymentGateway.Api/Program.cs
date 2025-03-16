using System.Diagnostics;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api;
using AspirePaymentGateway.Api.BankApi;
using AspirePaymentGateway.Api.Events;
using AspirePaymentGateway.Api.FraudApi;
using AspirePaymentGateway.Api.Storage;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using AspirePaymentGateway.Api.Telemetry;
using FluentValidation;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//domain-specific metrics
builder.Services.AddSingleton<BusinessMetrics>();
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddMeter(BusinessMetrics.Name));

// infrastructure
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddRefitClient<IFraudApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fraud-api"));
builder.Services.AddRefitClient<IBankApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://mock-bank-api"));

// classes
builder.Services.AddSingleton<IValidator<PaymentRequest>, PaymentRequestValidator>();
builder.Services.AddSingleton<IPaymentEventRepository, DynamoDbPaymentEventRepository>();

//builder.Services.ConfigureHttpJsonOptions(options =>
//{
//    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
//});

var app = builder.Build();

app.UseFluentValidationNamingFromJsonOptions();
app.MapDefaultEndpoints();
app.MapOpenApiForDevelopment("/scalar/v1");
app.UseHttpsRedirection();

app.MapPost("/payments",
async (
        IValidator<PaymentRequest> validator,
        IPaymentEventRepository repository,
        IFraudApi fraudApi,
        IBankApi bankApi,
        BusinessMetrics metrics,
        CancellationToken cancellationToken,
        PaymentRequest paymentRequest
    ) =>
    {
        // request validation

        var validationResult = await validator.ValidateAsync(paymentRequest);
        Activity.Current?.AddEvent(new ActivityEvent("Request validated"));

        if (!validationResult.IsValid)
        {
            metrics.RecordPaymentRequestRejected();
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var paymentRequested = new PaymentRequestedEvent($"pay_{Guid.NewGuid()}")
        {
            Amount = paymentRequest.Amount,
            CardNumber = paymentRequest.CardNumber,
            CardHolderName = paymentRequest.CardHolderName,
            Currency = paymentRequest.CurrencyCode,
            Cvv = paymentRequest.CVV,
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
        };

        metrics.RecordPaymentRequestAccepted();
        var saveResult = await repository.SaveAsync(paymentRequested, cancellationToken);


        // fraud checks

        var screeningRequest = new ScreeningRequest()
        {
            CardNumber = paymentRequest.CardNumber,
            CardHolderName = paymentRequest.CardHolderName,
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear
        };

        var screeningResponse = await fraudApi.DoScreening(screeningRequest, cancellationToken);
        Activity.Current?.AddEvent(new ActivityEvent("Request screened"));

        if (!screeningResponse.Accepted)
        {
            var paymentDeclined = new PaymentDeclinedEvent(paymentRequested.Id, DateTimeOffset.UtcNow) { Reason = "" };
            metrics.RecordPaymentFate(paymentDeclined.Action, paymentDeclined.Reason);

            saveResult = await repository.SaveAsync(paymentDeclined, cancellationToken);

            return Results.Created($"/payments/{paymentRequested.Id}", new PaymentResponse()
            {
                PaymentId = paymentRequested.Id,
                Status = PaymentStatus.Declined
            });
        }


        // authorisation

        var authorisationRequest = new AuthorisationRequest()
        {

        };

        var authorisationResponse = await bankApi.AuthoriseAsync(authorisationRequest, cancellationToken);
        Activity.Current?.AddEvent(new ActivityEvent("Request authorised"));

        var paymentAuthorised = new PaymentAuthorisedEvent(paymentRequested.Id) { AuthorisationCode = "abc" };
        metrics.RecordPaymentFate(paymentAuthorised.Action);

        saveResult = await repository.SaveAsync(paymentAuthorised, cancellationToken);

        return Results.Created($"/payments/{paymentRequested.Id}", new PaymentResponse()
        {
            PaymentId = paymentRequested.Id,
            Status = PaymentStatus.Authorised
        });
    })
    .WithSummary("Make Payment")
    .WithDescription("Makes a payment on the specified card: Fraud screening is performed before the request is sent to the bank for authorisation")
    .Produces<PaymentResponse>(StatusCodes.Status201Created)
    .ProducesValidationProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError);


app.MapGet("/payments/{paymentId}",
async (
    HttpContext httpContext,
    IPaymentEventRepository repository, 
    string paymentId, 
    CancellationToken cancellationToken) =>
{
    var getPaymentEventResults = await repository.GetAsync(paymentId, cancellationToken);
    return getPaymentEventResults.Match(
        paymentEvents =>
        {
            if (paymentEvents.Any())
            {
                return Results.Ok(new GetPaymentResponse() 
                { 
                    Events = paymentEvents.OrderBy(@event => @event.OccurredAt)
                });
            }

            return Results.NotFound(new Microsoft.AspNetCore.Mvc.ProblemDetails() 
            {
                //Type,
                //Title
                //Status
                Detail = $"Payment {paymentId} could not be found",
                Instance = httpContext.Request.Path,  
            });
        },
        storageError =>
        {
            return Results.Problem(statusCode: 500, title: "Internal Server Error")
            ;
        });
})
.WithName("GetPayment")
.Produces<GetPaymentResponse>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();