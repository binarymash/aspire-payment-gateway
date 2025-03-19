using System.ComponentModel.DataAnnotations;
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
using Microsoft.Extensions.Http.Resilience;
using MicroElements.OpenApi ;
using Refit;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

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
//builder.Services.AddSingleton<IValidator<PaymentRequest>, PaymentRequestValidator>();
builder.Services.AddSingleton<IPaymentEventRepository, DynamoDbPaymentEventRepository>();

// fluent validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationRulesToSwagger();

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
        //var validationResults = new List<ValidationResult>();
        //var context = new ValidationContext(paymentRequest);
        //var isValid = Validator.TryValidateObject(paymentRequest, context, validationResults, true);
        //Activity.Current?.AddEvent(new ActivityEvent("Request validated"));

        //if (!isValid)
        //{
        //    metrics.RecordPaymentRequestRejected();
        //    return Results.ValidationProblem(validationResults.Select(vr => new KeyValuePair<string, string[]>(vr.MemberNames.First(), [vr.ErrorMessage ?? string.Empty])));
        //}

        var validationResult = await validator.ValidateAsync(paymentRequest);
        Activity.Current?.AddEvent(new ActivityEvent("Request validated"));

        if (!validationResult.IsValid)
        {
            metrics.RecordPaymentRequestRejected();
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var paymentRequested = new PaymentRequestedEvent($"pay_{Guid.NewGuid()}")
        {
            Amount = paymentRequest.Payment.Amount,
            Currency = paymentRequest.Payment.CurrencyCode,
            CardNumber = paymentRequest.Card.CardNumber,
            CardHolderName = paymentRequest.Card.CardHolderName,
            Cvv = paymentRequest.Card.CVV,
            ExpiryMonth = paymentRequest.Card.Expiry.Month,
            ExpiryYear = paymentRequest.Card.Expiry.Year,
        };

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