using AspirePaymentGateway.FraudApi.Features.Screening;
using static AspirePaymentGateway.FraudApi.Features.Screening.Contracts;
using static Microsoft.Extensions.Hosting.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddOpenApi(options =>
    {
        // add authentication to OpenAPI spec
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    })
    .AddSingleton<ScreeningHandler>()
    .AddAuthorization(options => options.AddPolicy("ScreenPayment", configure => configure.RequireRole("screen-payment")))
    .AddKeycloakRoleClaimsTransformation()
    .AddAuthentication()
    .AddKeycloakJwtBearer("keycloak", realm: "fraud-api", options =>
    {
        options.RequireHttpsMetadata = false; //non-prod
        options.Audience = "account";
    });

var app = builder.Build();

// Middleware

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApiForDevelopment("/scalar/v1");

// Map endpoints

app.MapPost("/screening", (ScreeningHandler handler, ScreeningRequest request) => handler.Handle(request))
    .RequireAuthorization("ScreenPayment");

await app.RunAsync();
