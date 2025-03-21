namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.BankApi
{
    public interface IBankApi
    {
        [Refit.Post("/authorisation")]
        Task<AuthorisationResponse> AuthoriseAsync(AuthorisationRequest request, CancellationToken cancellationToken);
    }
}
