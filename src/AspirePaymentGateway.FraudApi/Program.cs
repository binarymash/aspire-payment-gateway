using AspirePaymentGateway.Api.FraudApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApiForDevelopment("/scalar/v1");

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapPost("/screening",
    (ScreeningRequest request) =>
    {
        if (request.CardNumber.EndsWith("11", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Ok(new ScreeningResponse() { Accepted = false });
        }

        if (request.CardNumber.EndsWith("12", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "Something bad happened");
        }

        if (request.CardNumber.EndsWith("13", StringComparison.OrdinalIgnoreCase))
        {
#pragma warning disable CA2201 // Do not raise reserved exception types
            throw new Exception("boom");
#pragma warning restore CA2201 // Do not raise reserved exception types
        }

        return Results.Ok(new ScreeningResponse() { Accepted = true });
    });

app.Run();
