using AspirePaymentGateway.Api.FraudApi;

namespace AspirePaymentGateway.Api.BankApi
{
    public interface IBankApi
    {
        [Refit.Post("/authorisation")]
        Task<AuthorisationResponse> AuthoriseAsync(AuthorisationRequest request, CancellationToken cancellationToken);
    }
}
