using AspirePaymentGateway.MockBankApi;
using Microsoft.AspNetCore.Mvc;
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

app.MapPost("/authorisation",
    (AuthorisationRequest request) =>
    {
        return Results.Created((string?)null, new AuthorisationResponse());
    })
    .WithDisplayName("Capture Payment")
    .WithDescription("Returns mocked responses for payment capture");

app.UseHttpsRedirection();

app.Run();
