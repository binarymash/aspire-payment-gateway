using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AspirePaymentGateway.Tests.Payments
{
    public class PaymentGatewayTestClient : IDisposable
    {
        private readonly HttpClient _paymentGatewayHttpClient;
        private IdentityServerTestClient _idServer;
        private bool disposedValue;

        public PaymentGatewayTestClient(HttpClient paymentGateway, IdentityServerTestClient idServer)
        {
            _paymentGatewayHttpClient = paymentGateway;
            _idServer = idServer;
        }

        public CreatePaymentRequestBuilder CreatePaymentRequest => new CreatePaymentRequestBuilder(this);

        public GetPaymentRequestBuilder GetPaymentRequest => new GetPaymentRequestBuilder(this);

        public class CreatePaymentRequestBuilder : RequestBuilder
        {
            public CreatePaymentRequestBuilder(PaymentGatewayTestClient testClient) : base(testClient)
            {
                RequestMessage.Method = HttpMethod.Post;
                RequestMessage.RequestUri = new Uri("/payments", UriKind.Relative);
                RequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            }

            public CreatePaymentRequestBuilder WithContent(Dictionary<string, object> requestDictionary)
            {
                return WithContent(JsonSerializer.Serialize(requestDictionary));
            }

            public CreatePaymentRequestBuilder WithContent(string requestJson)
            {
                RequestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                return this;
            }
        }

        public class GetPaymentRequestBuilder : RequestBuilder
        {
            public GetPaymentRequestBuilder(PaymentGatewayTestClient testClient) : base(testClient)
            {
                RequestMessage.Method = HttpMethod.Get;
                RequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            }
        }

        public abstract class RequestBuilder : IDisposable
        {
            protected HttpRequestMessage RequestMessage { get; init; }

            private readonly PaymentGatewayTestClient _testClient;
            private bool _authorise = true;
            private string? _token;
            private bool disposedValue;

            public RequestBuilder(PaymentGatewayTestClient testClient)
            {
                _testClient = testClient;
                RequestMessage = new HttpRequestMessage();
            }

            public RequestBuilder WithLocation(Uri? paymentLocation)
            {
                RequestMessage.RequestUri = paymentLocation;
                return this;
            }

            public RequestBuilder WithBearerToken(string token)
            {
                _authorise = true;
                _token = token;
                return this;
            }

            public RequestBuilder WithoutBearerToken()
            {
                _authorise = false;
                return this;
            }

            public async Task<HttpResponseMessage> SendAsync(CancellationToken ct)
            {
                if (_authorise)
                {
                    if (_token == null)
                    {
                        _token = await _testClient._idServer.GetPaymentGatewayTokenAsync(TestContext.Current.CancellationToken);
                    }
                    RequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                }

                return await _testClient._paymentGatewayHttpClient.SendAsync(RequestMessage, TestContext.Current.CancellationToken);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        RequestMessage.Dispose();
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
