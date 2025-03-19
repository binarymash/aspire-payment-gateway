using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api;
using AspirePaymentGateway.Api.BankApi;
using AspirePaymentGateway.Api.Events;
using AspirePaymentGateway.Api.Events.v4;
using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.FraudApi;
using AspirePaymentGateway.Api.Storage;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using AspirePaymentGateway.Api.Telemetry;
using FluentValidation;
using Microsoft.Extensions.Http.Resilience;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//domain-specific metrics
builder.Services.AddSingleton<BusinessMetrics>();
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddMeter(BusinessMetrics.Name));

// infrastructure
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
var fraudApiClientBuilder = builder.Services.AddRefitClient<IFraudApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fraud-api"));
//fraudApiClientBuilder.AddStandardResilienceHandler(options => 
//{
//    options.Retry.DisableForUnsafeHttpMethods();
//    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
//});
builder.Services.AddRefitClient<IBankApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://mock-bank-api"));

// classes
builder.Services.AddSingleton<IValidator<PaymentRequest>, PaymentRequestValidator>();
builder.Services.AddSingleton<IPaymentEventRepository, DynamoDbPaymentEventRepository>();
builder.Services.AddStandardDateTimeProvider();

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
        IDateTimeProvider dateTimeProvider,
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

        //v1
        //var paymentRequested = new PaymentRequestedEvent(
        //    paymentId: $"pay_{Guid.NewGuid()}",
        //    occurredAt: dateTimeProvider.UtcNowAsString,
        //    amount: paymentRequest.Payment.Amount,
        //    currency: paymentRequest.Payment.CurrencyCode,
        //    cardNumber: paymentRequest.Card.CardNumber,
        //    cardHolderName: paymentRequest.Card.CardHolderName,
        //    cvv: paymentRequest.Card.CVV,
        //    expiryMonth: paymentRequest.Card.Expiry.Month,
        //    expiryYear: paymentRequest.Card.Expiry.Year);

        //v2, v4
        var paymentRequested = new PaymentRequestedEvent
        {
            Id = $"pay_{Guid.NewGuid()}",
            OccurredAt = dateTimeProvider.UtcNowAsString,
            Amount = paymentRequest.Payment.Amount,
            Currency = paymentRequest.Payment.CurrencyCode,
            CardNumber = paymentRequest.Card.CardNumber,
            CardHolderName = paymentRequest.Card.CardHolderName,
            Cvv = paymentRequest.Card.CVV,
            ExpiryMonth = paymentRequest.Card.Expiry.Month,
            ExpiryYear = paymentRequest.Card.Expiry.Year
        };

        //v3
        //var paymentRequested = new PaymentRequestedEvent(
        //    Id: $"pay_{Guid.NewGuid()}",
        //    OccurredAt: dateTimeProvider.UtcNowAsString,
        //    Amount: paymentRequest.Payment.Amount,
        //    Currency: paymentRequest.Payment.CurrencyCode,
        //    CardNumber: paymentRequest.Card.CardNumber,
        //    CardHolderName: paymentRequest.Card.CardHolderName,
        //    Cvv: paymentRequest.Card.CVV,
        //    ExpiryMonth: paymentRequest.Card.Expiry.Month,
        //    ExpiryYear: paymentRequest.Card.Expiry.Year);


        metrics.RecordPaymentRequestAccepted();
        var saveResult = await repository.SaveAsync(paymentRequested, cancellationToken);

        // fraud checks

        var screeningRequest = new ScreeningRequest()
        {
            CardNumber = paymentRequest.Card.CardNumber,
            CardHolderName = paymentRequest.Card.CardHolderName,
            ExpiryMonth = paymentRequest.Card.Expiry.Month,
            ExpiryYear = paymentRequest.Card.Expiry.Year
        };

        var screeningResponse = await fraudApi.DoScreening(screeningRequest, cancellationToken);
        Activity.Current?.AddEvent(new ActivityEvent("Request screened"));

        if (!screeningResponse.Accepted)
        {
            //v1
            //var paymentDeclined = new PaymentDeclinedEvent(
            //    paymentId: paymentRequested.Id,
            //    occurredAt: dateTimeProvider.UtcNowAsString,
            //    reason: "");

            //v2, v4
            var paymentDeclined = new PaymentDeclinedEvent
            {
                Id = paymentRequested.Id,
                OccurredAt = dateTimeProvider.UtcNowAsString,
                Reason = ""
            };

            //v3
            //var paymentDeclined = new PaymentDeclinedEvent(
            //    Id: paymentRequested.Id,
            //    OccurredAt: dateTimeProvider.UtcNowAsString,
            //    Reason: "");

            metrics.RecordPaymentFate(paymentDeclined.Action, paymentDeclined.Reason);

            saveResult = await repository.SaveAsync(paymentDeclined, cancellationToken);

            return Results.Created($"/payments/{paymentRequested.Id}", new PaymentResponse(paymentRequested.Id, PaymentStatus.Declined));
        }

        // authorisation

        var authorisationRequest = new AuthorisationRequest()
        {

        };

        var authorisationResponse = await bankApi.AuthoriseAsync(authorisationRequest, cancellationToken);
        Activity.Current?.AddEvent(new ActivityEvent("Request authorised"));

        //v1
        //var paymentAuthorised = new PaymentAuthorisedEvent(
        //    paymentId: paymentRequested.Id,
        //    occurredAt: dateTimeProvider.UtcNowAsString,
        //    authorisationCode: "abc");

        //v2, v4
        var paymentAuthorised = new PaymentAuthorisedEvent
        {
            Id = paymentRequested.Id,
            OccurredAt = dateTimeProvider.UtcNowAsString,
            AuthorisationCode = "abc"
        };

        //v3
        //var paymentAuthorised = new PaymentAuthorisedEvent(
        //    Id: paymentRequested.Id,
        //    OccurredAt: dateTimeProvider.UtcNowAsString,
        //    AuthorisationCode: "abc");

        metrics.RecordPaymentFate(paymentAuthorised.Action);

        saveResult = await repository.SaveAsync(paymentAuthorised, cancellationToken);

        return Results.Created($"/payments/{paymentRequested.Id}", new PaymentResponse(paymentRequested.Id, PaymentStatus.Authorised));    })
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
                return Results.Ok(new GetPaymentResponse(paymentEvents.OrderBy(@event => @event.OccurredAt)));
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