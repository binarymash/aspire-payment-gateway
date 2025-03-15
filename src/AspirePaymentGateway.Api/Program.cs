using AspirePaymentGateway.Api;
using AspirePaymentGateway.Api.BankApi;
using AspirePaymentGateway.Api.FraudApi;
using AspirePaymentGateway.Api.Storage;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using Amazon.DynamoDBv2;
using FluentValidation;
using Scalar.AspNetCore;
using Refit;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Events;
using AspirePaymentGateway.Api.Telemetry;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//domain-specific metrics
builder.Services.AddSingleton<BusinessMetrics>();
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddMeter(BusinessMetrics.Name));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// infrastructure
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddRefitClient<IFraudApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fraud-api"));
builder.Services.AddRefitClient<IBankApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://mock-bank-api"));

// classes
builder.Services.AddSingleton<IValidator<PaymentRequest>, PaymentRequestValidator>();
builder.Services.AddSingleton<IPaymentRepository, DynamoDbPaymentRepository>();
builder.Services.AddSingleton<IPaymentEventRepository, DynamoDbPaymentEventRepository>();


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    //see https://github.com/dotnet/aspnetcore/issues/57332
    // app.MapScalarApiReference();
    app.MapScalarApiReference(_ => _.Servers = []);

    app.MapGet("",
        [ExcludeFromDescription]
        () =>
        {
            return Results.Redirect("/scalar/v1");
        });
}

app.UseHttpsRedirection();

app.MapPost("/payments",
    [ProducesResponseType(typeof(PaymentResponse), 201)]
    [ProducesErrorResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    async (
        IValidator<PaymentRequest> validator,
        IPaymentRepository repository,
        IPaymentEventRepository repository2,
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

        //var saveResult = await repository.SavePaymentRequestAsync(paymentId, paymentRequest, cancellationToken);

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
        var saveResult = await repository2.SaveAsync(paymentRequested, cancellationToken);


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
            //saveResult = await repository.SavePaymentResultAsync(paymentId, PaymentStatus.Declined, cancellationToken);
            var paymentDeclined = new PaymentDeclinedEvent(paymentRequested.Id, DateTimeOffset.UtcNow) { Reason = "" };
            metrics.RecordPaymentFate(paymentDeclined.Action, paymentDeclined.Reason);

            saveResult = await repository2.SaveAsync(paymentDeclined, cancellationToken);

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

        //saveResult = await repository.SavePaymentResultAsync(paymentId, PaymentStatus.Authorised, cancellationToken);

        var paymentAuthorised = new PaymentAuthorisedEvent(paymentRequested.Id) { AuthorisationCode = "abc" };
        metrics.RecordPaymentFate(paymentAuthorised.Action);

        saveResult = await repository2.SaveAsync(paymentAuthorised, cancellationToken);

        return Results.Created($"/payments/{paymentRequested.Id}", new PaymentResponse()
        {
            PaymentId = paymentRequested.Id,
            Status = PaymentStatus.Authorised
        });
    })
    .WithName("PostPayment")
    .WithDescription("Makes a payment")
    .WithSummary("This is a summary of the endpoint");


app.MapGet("/payments/{paymentId}",
async (IPaymentEventRepository repository, string paymentId, CancellationToken cancellationToken) =>
{
    var getPaymentEventResults = await repository.GetAsync(paymentId, cancellationToken);
    return getPaymentEventResults.Match(
        paymentEvents =>
        {
            return Results.Ok(new GetPaymentResponse() { Events = paymentEvents.OrderBy(@event => @event.OccurredAt) });
        },
        storageError =>
        {
            return Results.Problem(statusCode: 500, title: "Internal Server Error");
        });
})
.WithName("GetPayment");


app.Run();