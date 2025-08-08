using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using AspirePaymentGateway.Api.Features.Payments.Validation;
using BinaryMash.Extensions.Results;

namespace AspirePaymentGateway.Api.Features.Payments
{
    public partial class GetPaymentHandler(PaymentSession session, ILogger<GetPaymentHandler> logger, PaymentIdValidator validator)
    {
        // HTTP concerns

        public async Task<IResult> GetPaymentAsync(string paymentId, CancellationToken cancellationToken)
        {
            var result = await RunDomainWorkflowAsync(paymentId, cancellationToken);

            return result.Match(
                onSuccess: Map200PaymentResponse,
                onFailure: MapFailureResponse);
        }

        private IResult Map200PaymentResponse(Result<Payment> success)
        {
            return Results.Ok(Contracts.MapPaymentResponse(success.Value));
        }

        private IResult MapFailureResponse(Result<Payment> failure)
        {
            if (failure.ErrorDetail is Errors.ExceptionError exceptionError)
            {
                LogExceptionWhenRetrievingPayment(exceptionError.Exception);
            }

            return failure.ErrorDetail switch
            {
                // 400 bad request
                Errors.ValidationError validationError => Results.ValidationProblem(errors: validationError.ValidationResult.ToDictionary()),

                // 404 not found
                Errors.PaymentNotFoundError paymentNotFoundError => Results.NotFound(new Microsoft.AspNetCore.Mvc.ProblemDetails()
                {
                    //Type,
                    //Title
                    //Status
                    Detail = $"Payment {paymentNotFoundError.PaymentId} could not be found",
                }),

                // 500 internal server error
                _ => Results.Problem(statusCode: 500, title: "Internal Server Error")
            };
        }

        // Domain Workflow

        private async Task<Result<Payment>> RunDomainWorkflowAsync(string paymentId, CancellationToken ct)
        {
            var validationResult = validator.Validate(paymentId);
            if (!validationResult.IsValid)
            {
                return Result.Failure<Payment>(new Errors.ValidationError(validationResult));
            }

            return await session.GetAsync(paymentId, ct);
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retrieve payment events")]
        partial void LogExceptionWhenRetrievingPayment(Exception exception);
    }
}
