using BinaryMash.Extensions.OAuth2.AuthorizationProvider;
using Microsoft.Extensions.Options;

namespace AspirePaymentGateway.Api.Features.Payments.Services.FraudApi
{
    public class FraudApiTokenProvider(HttpClient httpClient, IOptions<AuthorizationOptions> options) : AuthorizationProvider(httpClient, options);
}
