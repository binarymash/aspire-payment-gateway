using Aspire.Hosting;
using Microsoft.Extensions.Logging;

namespace AspirePaymentGateway.Tests.Payments
{
    public class PaymentFixture : IDisposable
    {
        public PaymentGatewayTestClient PaymentGateway { get; init; }
        public IdentityServerTestClient IdentityServer { get; init; }

        private readonly DistributedApplication _app;
        private bool disposedValue;

        public PaymentFixture()
        {
            var appHost = DistributedApplicationTestingBuilder
                .CreateAsync<Projects.AspirePaymentGateway_AppHost>(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();

            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                // Override the logging filters from the app's configuration
                logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
                // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
            });

            _app = appHost.Build();

            _app.StartAsync(TestContext.Current.CancellationToken).GetAwaiter().GetResult();

            Task.WaitAll(
                _app.ResourceNotifications.WaitForResourceHealthyAsync("payment-gateway", TestContext.Current.CancellationToken),
                _app.ResourceNotifications.WaitForResourceHealthyAsync("keycloak", TestContext.Current.CancellationToken));

            var keycloakClient = _app.CreateHttpClient("keycloak");

            IdentityServer = new IdentityServerTestClient(keycloakClient);
            PaymentGateway = new PaymentGatewayTestClient(_app.CreateHttpClient("payment-gateway", "https"), IdentityServer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _app.Dispose();
                    PaymentGateway.Dispose();
                    IdentityServer.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
