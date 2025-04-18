namespace AspirePaymentGateway.MockBankApi.Features.Authorisation
{
    public class AuthorisationHandler
    {
        public IResult Handle(Contracts.AuthorisationRequest request)
        {
            if (request.AuthorisationRequestId.EndsWith("99", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok(new Contracts.AuthorisationResponse(request.AuthorisationRequestId, false));
            }

            if (request.AuthorisationRequestId.EndsWith("88", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Problem(detail: "Something bad happened", statusCode: StatusCodes.Status500InternalServerError);
            }

            if (request.AuthorisationRequestId.EndsWith("77", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("boom");
            }

            return Results.Ok(new Contracts.AuthorisationResponse(request.AuthorisationRequestId, true, Guid.NewGuid().ToString()[..8]));
        }
    }
}
