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

        public class CreatePaymentRequestBuilder : IDisposable
        {
            private readonly PaymentGatewayTestClient testClient;
            private readonly HttpRequestMessage _requestMessage;
            private bool _authorise = true;
            private string? _token;
            private bool disposedValue;

            public CreatePaymentRequestBuilder(PaymentGatewayTestClient testClient)
            {
                this.testClient = testClient;
                
                _requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("/payments", UriKind.Relative),
                };

                _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            }

            public CreatePaymentRequestBuilder WithContent(Dictionary<string, object> requestDictionary)
            {
                return WithContent(JsonSerializer.Serialize(requestDictionary));
            }

            public CreatePaymentRequestBuilder WithContent(string requestJson)
            {
                _requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                return this;
            }

            public CreatePaymentRequestBuilder WithBearerToken(string token)
            {
                _authorise = true;
                _token = token;
                return this;
            }

            public CreatePaymentRequestBuilder WithoutBearerToken()
            {
                _authorise = false;
                _token = null;
                return this;
            }

            public async Task<HttpResponseMessage> SendAsync(CancellationToken ct)
            {
                if (_authorise)
                {
                    if (_token == null)
                    {
                        _token = await testClient._idServer.GetPaymentGatewayTokenAsync(TestContext.Current.CancellationToken);
                    }
                    _requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                }

                return await testClient._paymentGatewayHttpClient.SendAsync(_requestMessage, TestContext.Current.CancellationToken);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _requestMessage.Dispose();
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

        public class GetPaymentRequestBuilder : IDisposable
        {
            private readonly PaymentGatewayTestClient testClient;
            private readonly HttpRequestMessage _requestMessage;
            private bool _authorise = true;
            private string? _token;
            private bool disposedValue;

            public GetPaymentRequestBuilder(PaymentGatewayTestClient testClient)
            {
                this.testClient = testClient;

                _requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                };
            }

            public GetPaymentRequestBuilder WithLocation(Uri? paymentLocation)
            {
                _requestMessage.RequestUri = paymentLocation;
                return this;
            }

            public GetPaymentRequestBuilder WithBearerToken(string token)
            {
                _authorise = true;
                _token = token;
                return this;
            }

            public GetPaymentRequestBuilder WithoutBearerToken()
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
                        _token = await testClient._idServer.GetPaymentGatewayTokenAsync(TestContext.Current.CancellationToken);
                    }
                    _requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                }

                return await testClient._paymentGatewayHttpClient.SendAsync(_requestMessage, TestContext.Current.CancellationToken);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _requestMessage.Dispose();
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
