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
        if (request.CardNumber.EndsWith("11"))
        {
            return Results.Ok(new ScreeningResponse() { Accepted = false });
        }

        if (request.CardNumber.EndsWith("12"))
        {
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "Something bad happened");
        }

        if (request.CardNumber.EndsWith("13"))
        {
            throw new Exception("boom");
        }

        return Results.Ok(new ScreeningResponse() { Accepted = true });
    });

app.Run();
