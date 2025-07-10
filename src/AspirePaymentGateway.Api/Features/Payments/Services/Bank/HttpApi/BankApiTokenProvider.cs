using BinaryMash.Extensions.OAuth2.ClientCredentialsAuthorizationProvider;
using Microsoft.Extensions.Options;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Bank.HttpApi
{
    public class BankApiTokenProvider(HttpClient httpClient, IOptions<AuthorizationOptions> options) : AuthorizationProvider(httpClient, options);
}
