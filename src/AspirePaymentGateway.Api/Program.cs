using AspirePaymentGateway.Api;
using AspirePaymentGateway.Api.Features.Payments;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;
using static Microsoft.Extensions.Hosting.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApiServices()
    .AddDomainServices()
    .AddInfrastructure();

var app = builder.Build();

// Middleware

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.UseFluentValidationNamingFromJsonOptions();
app.MapOpenApiForDevelopment("/scalar/v1");
app.UseHttpsRedirection();

// Map endpoints

app.MapPost("/payments",
        async (CreatePaymentHandler handler, PaymentRequest request, CancellationToken cancellationToken) => await handler.PostPaymentAsync(request, cancellationToken))
    .WithSummary("Make Payment")
    .WithDescription("Makes a payment on the specified card: Fraud screening is performed before the request is sent to the bank for authorisation")
    .Produces<PaymentResponse>(StatusCodes.Status201Created)
    .ProducesValidationProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .RequireAuthorization();

app.MapGet("/payments/{paymentId}",
        async (GetPaymentHandler handler, string paymentId, CancellationToken cancellationToken) => await handler.GetPaymentAsync(paymentId, cancellationToken))
    .WithSummary("Get Payment")
    .WithDescription("Retrieves the payment events for the specified payment")
    .Produces<PaymentResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .RequireAuthorization();

await app.RunAsync();