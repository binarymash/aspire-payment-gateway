using System.Net.Http.Headers;
using System.Text;

namespace AspirePaymentGateway.Tests.Payments
{
    public class PaymentGatewayTestClient : IDisposable
    {
        private readonly HttpClient _paymentGatewayHttpClient;
        private IdentityServerTestClient _idServer;
        private bool disposedValue;

        public HttpClient HttpClient => _paymentGatewayHttpClient;

        public PaymentGatewayTestClient(HttpClient paymentGateway, IdentityServerTestClient idServer)
        {
            _paymentGatewayHttpClient = paymentGateway;
            _idServer = idServer;
        }

        public async Task<HttpResponseMessage> CreatePaymentAsync(string createPaymentRequest, CancellationToken ct)
        {
            var token = await _idServer.GetPaymentGatewayTokenAsync(TestContext.Current.CancellationToken);

            using var createPaymentMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("/payments", UriKind.Relative),
                Content = new StringContent(createPaymentRequest, Encoding.UTF8, "application/json")
            };

            createPaymentMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            createPaymentMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            return await _paymentGatewayHttpClient.SendAsync(createPaymentMessage, TestContext.Current.CancellationToken);
        }

        public async Task<HttpResponseMessage> GetPaymentAsync(Uri paymentUri, CancellationToken ct)
        {
            var token = await _idServer.GetPaymentGatewayTokenAsync(TestContext.Current.CancellationToken);

            using var getPaymentMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = paymentUri,
            };

            getPaymentMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            getPaymentMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _paymentGatewayHttpClient.SendAsync(getPaymentMessage, TestContext.Current.CancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _paymentGatewayHttpClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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
