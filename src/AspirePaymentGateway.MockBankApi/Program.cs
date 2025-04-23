using AspirePaymentGateway.MockBankApi.Features.Authorisation;
using static Microsoft.Extensions.Hosting.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddOpenApi(options =>
        {
            // add authentication to OpenAPI spec
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        })
    .AddSingleton<AuthorisationHandler>()
    .AddAuthorization(options => options.AddPolicy("PaymentCapture", policy => policy.RequireRole("payment:capture")))
    .AddKeycloakRoleClaimsTransformation()
    .AddAuthentication()
    .AddKeycloakJwtBearer("keycloak", realm: "bank-api", options =>
    {
        options.RequireHttpsMetadata = false; //non-prod
        options.Audience = "account";
    });

// Add services to the container.

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApiForDevelopment("/scalar/v1");

app.MapPost("/authorisation", (Contracts.AuthorisationRequest request, AuthorisationHandler handler) => handler.Handle(request))
    .WithDisplayName("Capture Payment")
    .WithDescription("Returns mocked responses for payment capture")
    .RequireAuthorization("PaymentCapture");

app.UseHttpsRedirection();

await app.RunAsync();
