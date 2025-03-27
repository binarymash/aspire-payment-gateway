namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.BankApi
{
    public interface IBankApi
    {
        [Refit.Post("/authorisation")]
        Task<Contracts.AuthorisationResponse> AuthoriseAsync(Contracts.AuthorisationRequest request, CancellationToken cancellationToken);
    }
}
