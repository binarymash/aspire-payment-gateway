namespace AspirePaymentGateway.Api.Features.Payments.Services.BankApi
{
    public interface IBankApi
    {
        [Refit.Post("/authorisation")]
        Task<Contracts.AuthorisationResponse> AuthoriseAsync(Contracts.AuthorisationRequest request, CancellationToken cancellationToken);
    }
}
