namespace AspirePaymentGateway.Api.Extensions.Http.Logging
{
    public partial class AuthDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //TODO: implement token-based authentication with KeyCloak

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
