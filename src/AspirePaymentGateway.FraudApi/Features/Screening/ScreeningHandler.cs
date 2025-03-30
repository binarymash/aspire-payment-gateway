using static AspirePaymentGateway.FraudApi.Features.Screening.Contracts;

namespace AspirePaymentGateway.FraudApi.Features.Screening
{
    public class ScreeningHandler
    {
        public IResult Handle(ScreeningRequest request)
        {
            if (request.CardNumber.EndsWith("99", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok(new ScreeningResponse() { Accepted = false });
            }

            if (request.CardNumber.EndsWith("88", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "Something bad happened");
            }

            if (request.CardNumber.EndsWith("77", StringComparison.OrdinalIgnoreCase))
            {
#pragma warning disable CA2201 // Do not raise reserved exception types
                throw new Exception("boom");
#pragma warning restore CA2201 // Do not raise reserved exception types
            }

            return Results.Ok(new ScreeningResponse() { SomeNumber = 123, Accepted = true });
        }
    }
}
