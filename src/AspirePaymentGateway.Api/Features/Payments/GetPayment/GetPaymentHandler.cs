using AspirePaymentGateway.Api.Features.Payments.GetPayment.EventStore;
using static AspirePaymentGateway.Api.Features.Payments.GetPayment.Contracts;

namespace AspirePaymentGateway.Api.Features.Payments.GetPayment
{
    public class GetPaymentHandler(IGetPaymentEvent repository)
    {
        public async Task<IResult> GetPaymentAsync(HttpContext httpContext, string paymentId, CancellationToken cancellationToken)
        {
            var getPaymentEventResults = await repository.GetAsync(paymentId, cancellationToken);

            return getPaymentEventResults.Match(
                paymentEvents =>
                {
                    if (paymentEvents.Any())
                    {
                        return Results.Ok(new GetPaymentResponse(paymentEvents.OrderBy(@event => @event.OccurredAt)));
                    }

                    return Results.NotFound(new Microsoft.AspNetCore.Mvc.ProblemDetails()
                    {
                        //Type,
                        //Title
                        //Status
                        Detail = $"Payment {paymentId} could not be found",
                        Instance = httpContext.Request.Path,
                    });
                },
                storageError =>
                {
                    return Results.Problem(statusCode: 500, title: "Internal Server Error")
                    ;
                });
        }
    }
}
