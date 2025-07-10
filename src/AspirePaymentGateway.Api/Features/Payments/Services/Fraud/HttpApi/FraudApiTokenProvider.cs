using BinaryMash.Extensions.OAuth2.ClientCredentialsAuthorizationProvider;
using Microsoft.Extensions.Options;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Fraud.HttpApi;

public class FraudApiTokenProvider(HttpClient httpClient, IOptions<AuthorizationOptions> options) : AuthorizationProvider(httpClient, options);
