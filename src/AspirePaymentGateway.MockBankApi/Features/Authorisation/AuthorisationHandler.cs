namespace AspirePaymentGateway.MockBankApi.Features.Authorisation
{
    public class AuthorisationHandler
    {
        public IResult HandleAsync(Contracts.AuthorisationRequest request, CancellationToken ct)
        {
            if (request.AuthorisationRequestId.EndsWith("99", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok(new Contracts.AuthorisationResponse(request.AuthorisationRequestId, false));
            }

            if (request.AuthorisationRequestId.EndsWith("88", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "Something bad happened");
            }

            if (request.AuthorisationRequestId.EndsWith("77", StringComparison.OrdinalIgnoreCase))
            {
#pragma warning disable CA2201 // Do not raise reserved exception types
                throw new Exception("boom");
#pragma warning restore CA2201 // Do not raise reserved exception types
            }

            return Results.Ok(new Contracts.AuthorisationResponse(request.AuthorisationRequestId, true, Guid.NewGuid().ToString().Substring(0,8)));
        }
    }
}
