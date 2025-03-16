using AspirePaymentGateway.MockBankApi;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApiForDevelopment("/scalar/v1");

app.MapPost("/authorisation",
    (AuthorisationRequest request) =>
    {
        return Results.Created((string?)null, new AuthorisationResponse());
    })
    .WithDisplayName("Capture Payment")
    .WithDescription("Returns mocked responses for payment capture");

app.UseHttpsRedirection();

app.Run();
