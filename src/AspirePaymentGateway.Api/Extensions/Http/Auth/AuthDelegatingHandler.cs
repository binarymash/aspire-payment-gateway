namespace AspirePaymentGateway.Api.Extensions.Http.Logging
{
    public class AuthDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //TODO: implement token-based authentication with KeyCloak

            return base.SendAsync(request, cancellationToken);
        }
    }
}
