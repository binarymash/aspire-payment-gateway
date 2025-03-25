using AspirePaymentGateway.Api.Features.Payments.GetPayment.EventStore;
using static AspirePaymentGateway.Api.Features.Payments.GetPayment.Contracts;

namespace AspirePaymentGateway.Api.Features.Payments.GetPayment
{
    public partial class GetPaymentHandler(IGetPaymentEvent repository, ILogger<GetPaymentHandler> logger)
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
                exception =>
                {
                    LogExceptionWhenRetrievingPayment(exception);
                    return Results.Problem(statusCode: 500, title: "Internal Server Error");
                });
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retrieve payment events")]
        partial void LogExceptionWhenRetrievingPayment(Exception exception);
    }
}
