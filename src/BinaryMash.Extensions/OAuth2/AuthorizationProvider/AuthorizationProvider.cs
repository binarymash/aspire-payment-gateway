using Duende.IdentityModel.Client;
using Microsoft.Extensions.Options;
using static Duende.IdentityModel.OidcConstants;

namespace BinaryMash.Extensions.OAuth2.AuthorizationProvider
{
    /// <summary>
    /// Naive implementation to retrieve a token using Client Credentials
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="options"></param>
    public abstract class AuthorizationProvider(HttpClient httpClient, IOptions<AuthorizationOptions> options)
    {
        readonly AuthorizationOptions _options = options.Value;

        public async Task<string> GetClientCredentialAccessTokenAsync(HttpRequestMessage message, CancellationToken ct)
        {
            var address = $"{httpClient.BaseAddress}realms/{_options.Realm}/protocol/openid-connect/token";

            ClientCredentialsTokenRequest request = new()
            {
                Address = address,
                GrantType = GrantTypes.ClientCredentials,
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                //Scope = _options.Scope,
            };

            var response = await httpClient.RequestClientCredentialsTokenAsync(request, ct);

            return response.AccessToken!;
        }
    }
}
