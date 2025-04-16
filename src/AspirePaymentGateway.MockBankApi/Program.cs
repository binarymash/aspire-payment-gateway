using AspirePaymentGateway.MockBankApi.Features.Authorisation;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<AuthorisationHandler>();

// Add services to the container.

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApiForDevelopment("/scalar/v1");

app.MapPost("/authorisation", (Contracts.AuthorisationRequest request, CancellationToken ct, AuthorisationHandler handler) => handler.HandleAsync(request, ct))
    .WithDisplayName("Capture Payment")
    .WithDescription("Returns mocked responses for payment capture");

app.UseHttpsRedirection();

app.Run();
