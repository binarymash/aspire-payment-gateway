using AspirePaymentGateway.Api.FraudApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.MapPost("/screening", 
    () =>
    {
        var response = new ScreeningResponse() { Accepted = true };
        return Results.Ok(response);
    });

app.Run();
