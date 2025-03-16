using AspirePaymentGateway.Api.FraudApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApiForDevelopment("/scalar/v1");

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapPost("/screening", 
    () =>
    {
        var response = new ScreeningResponse() { Accepted = true };
        return Results.Ok(response);
    });

app.Run();
