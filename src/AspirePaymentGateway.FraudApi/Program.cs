using AspirePaymentGateway.FraudApi.Features.Screening;
using static AspirePaymentGateway.FraudApi.Features.Screening.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApiForDevelopment("/scalar/v1");

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapPost("/screening",
    (ScreeningHandler handler, ScreeningRequest request) => handler.Handle(request)
);

app.Run();
