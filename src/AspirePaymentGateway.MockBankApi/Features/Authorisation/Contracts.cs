namespace AspirePaymentGateway.MockBankApi.Features.Authorisation
{
    public static class Contracts
    {
        public record AuthorisationRequest(string AuthorisationRequestId);

        public record AuthorisationResponse(string AuthorisationRequestId, bool Authorised, string? AuthorisationCode = null);
    }
}
