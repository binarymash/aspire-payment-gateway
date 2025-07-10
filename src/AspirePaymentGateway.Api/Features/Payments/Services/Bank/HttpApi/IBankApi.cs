using Refit;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Bank.HttpApi
{
    [Headers("Authorization: Bearer")]
    public interface IBankApi
    {
        [Post("/authorisation")]
        Task<Contracts.AuthorisationResponse> AuthoriseAsync(Contracts.AuthorisationRequest request, CancellationToken cancellationToken);
    }
}
