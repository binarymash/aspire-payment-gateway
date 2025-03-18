using Microsoft.Extensions.Logging;

namespace AspirePaymentGateway.Tests.Tests;

public class IntegrationTest1
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task GetUnknownPaymentReturns404NotFound()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspirePaymentGateway_AppHost>();
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync().WaitAsync(DefaultTimeout);
        await app.StartAsync().WaitAsync(DefaultTimeout);

        // Act
        var httpClient = app.CreateHttpClient("payment-gateway");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("payment-gateway").WaitAsync(DefaultTimeout);
        var response = await httpClient.GetAsync("/payments/abcdefg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
