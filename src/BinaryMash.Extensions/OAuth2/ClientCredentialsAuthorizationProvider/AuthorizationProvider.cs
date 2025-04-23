using Duende.IdentityModel.Client;
using Microsoft.Extensions.Options;
using static Duende.IdentityModel.OidcConstants;

namespace BinaryMash.Extensions.OAuth2.ClientCredentialsAuthorizationProvider
{
    /// <summary>
    /// Naive implementation to retrieve a token using Client Credentials
    /// </summary>
    public abstract class AuthorizationProvider : IDisposable
    {
        readonly HttpClient _httpClient;
        readonly AuthorizationOptions _options;
        private readonly ClientCredentialsTokenRequest _request;

        System.DateTime _renewAfter;
        string _token;
        private bool disposedValue;

        protected AuthorizationProvider(HttpClient httpClient, IOptions<AuthorizationOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            _request = new()
            {
                Address = $"{httpClient.BaseAddress}realms/{_options.Realm}/protocol/openid-connect/token",
                GrantType = GrantTypes.ClientCredentials,
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
            };

            _renewAfter = System.DateTime.MinValue;
            _token = string.Empty;
        }

        public async Task<string> GetClientCredentialAccessTokenAsync(HttpRequestMessage _, CancellationToken ct)
        {
            if (System.DateTime.UtcNow >= _renewAfter)
            {
                await RequestFromIdentityServerAsync(ct);
            }

            return _token;
        }

        private async Task RequestFromIdentityServerAsync(CancellationToken ct)
        {
            var response = await _httpClient.RequestClientCredentialsTokenAsync(_request, ct);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _renewAfter = System.DateTime.UtcNow.AddSeconds(response.ExpiresIn - _options.RenewalPeriodBeforeExpiryInSeconds);
                _token = response.AccessToken!;
            }
            else
            {
                throw new TokenRetrievalException($"Error retrieving token: {response.Error}");
            }
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
