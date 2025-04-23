using BinaryMash.Extensions.OAuth2.ClientCredentialsAuthorizationProvider;
using Microsoft.Extensions.Options;

namespace AspirePaymentGateway.Api.Features.Payments.Services.BankApi
{
    public class BankApiTokenProvider(HttpClient httpClient, IOptions<AuthorizationOptions> options) : AuthorizationProvider(httpClient, options);
}
