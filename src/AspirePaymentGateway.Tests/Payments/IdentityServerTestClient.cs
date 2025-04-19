using Duende.IdentityModel.Client;

namespace AspirePaymentGateway.Tests.Payments
{
    public class IdentityServerTestClient(HttpClient httpClient) : IDisposable
    {
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    httpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        public async Task<string> GetPaymentGatewayTokenAsync(CancellationToken ct)
        {
            var address = $"{httpClient.BaseAddress}realms/payment-gateway/protocol/openid-connect/token";

            PasswordTokenRequest request = new()
            {
                Address = address,
                GrantType = "password",
                ClientId = "payment-gateway-customer",
                Scope = "email openid",
                UserName = "test@test.com",
                Password = "123"
            };

            var response = await httpClient.RequestPasswordTokenAsync(request, ct);

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
