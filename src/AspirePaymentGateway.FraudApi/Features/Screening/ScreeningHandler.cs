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
                return Results.Problem(detail: "Something bad happened", statusCode: StatusCodes.Status500InternalServerError);
            }

            if (request.CardNumber.EndsWith("77", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("boom");
            }

            return Results.Ok(new ScreeningResponse() { SomeNumber = 123, Accepted = true });
        }
    }
}
