using Duende.IdentityModel.Client;

namespace AspirePaymentGateway.Tests.Payments
{
    public class IdentityServerTestClient : IDisposable
    {
        private HttpClient _httpClient;
        private bool disposedValue;

        public IdentityServerTestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        public async Task<string> GetPaymentGatewayTokenAsync(CancellationToken ct)
        {
            var address = $"{_httpClient.BaseAddress}realms/payment-gateway/protocol/openid-connect/token";

            PasswordTokenRequest request = new()
            {
                Address = address,
                GrantType = "password",
                ClientId = "payment-gateway-customer",
                Scope = "email openid",
                UserName = "test@test.com",
                Password = "123"
            };

            var response = await _httpClient.RequestPasswordTokenAsync(request, TestContext.Current.CancellationToken);

            return response.AccessToken!;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
