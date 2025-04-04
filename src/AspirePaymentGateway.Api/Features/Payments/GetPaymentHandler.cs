using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;

namespace AspirePaymentGateway.Api.Features.Payments
{
    public partial class GetPaymentHandler(PaymentSession session, ILogger<GetPaymentHandler> logger)
    {
        public async Task<IResult> GetPaymentAsync(string paymentId, CancellationToken cancellationToken)
        {
            var result = await session.GetAsync(paymentId, cancellationToken);

            if (result.IsSuccess)
            {
                return Results.Ok(Contracts.MapPaymentResponse(result.Value));
            }

            if (result.ErrorDetail is Errors.PaymentNotFoundError)
            {
                return Results.NotFound(new Microsoft.AspNetCore.Mvc.ProblemDetails()
                {
                    //Type,
                    //Title
                    //Status
                    Detail = $"Payment {paymentId} could not be found",
                });
            }

            if (result.ErrorDetail is Errors.ExceptionError)
            {
                LogExceptionWhenRetrievingPayment((result.ErrorDetail as Errors.ExceptionError).exception);

            }

            return Results.Problem(statusCode: 500, title: "Internal Server Error");
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retrieve payment events")]
        partial void LogExceptionWhenRetrievingPayment(Exception exception);
    }
}
