using Refit;

namespace AspirePaymentGateway.Api.Features.Payments.Services.BankApi
{
    [Headers("Authorization: Bearer")]
    public interface IBankApi
    {
        [Refit.Post("/authorisation")]
        Task<Contracts.AuthorisationResponse> AuthoriseAsync(Contracts.AuthorisationRequest request, CancellationToken cancellationToken);
    }
}
