using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AspirePaymentGateway.Tests.Payments
{
    public class PaymentGatewayTestClient(HttpClient PaymentGateway, IdentityServerTestClient IdServer) : IDisposable
    {
        private bool disposedValue;

        public CreatePaymentRequestBuilder CreatePaymentRequest => new(PaymentGateway, IdServer);

        public GetPaymentRequestBuilder GetPaymentRequest => new(PaymentGateway, IdServer);

        public class CreatePaymentRequestBuilder : RequestBuilder
        {
            public CreatePaymentRequestBuilder(HttpClient PaymentGateway, IdentityServerTestClient IdServer) : base(PaymentGateway, IdServer)
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
            public GetPaymentRequestBuilder(HttpClient PaymentGateway, IdentityServerTestClient IdServer) : base(PaymentGateway, IdServer)
            {
                RequestMessage.Method = HttpMethod.Get;
                RequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            }
        }

        public abstract class RequestBuilder(HttpClient paymentGateway, IdentityServerTestClient idServer) : IDisposable
        {
            protected HttpRequestMessage RequestMessage { get; } = new();

            private bool _authorise = true;
            private string? _token;
            private bool disposedValue;

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
                    _token ??= await idServer.GetPaymentGatewayTokenAsync(ct);
                    RequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                }

                return await paymentGateway.SendAsync(RequestMessage, ct);
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
                    PaymentGateway.Dispose();
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
