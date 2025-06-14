﻿using AspirePaymentGateway.Api.Features.Payments.Domain;
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

            // 200 ok
            if (result.IsSuccess)
            {
                return Results.Ok(Contracts.MapPaymentResponse(result.Value));
            }

            // 400 bad request
            switch (result.ErrorDetail)
            {
                case Errors.ValidationError:
                    return Results.ValidationProblem(errors: (result.ErrorDetail as Errors.ValidationError)!.ValidationResult.ToDictionary());
                case Errors.PaymentNotFoundError:
                    return Results.NotFound(new Microsoft.AspNetCore.Mvc.ProblemDetails()
                    {
                        //Type,
                        //Title
                        //Status
                        Detail = $"Payment {paymentId} could not be found",
                    });
                case Errors.ExceptionError:
                    LogExceptionWhenRetrievingPayment((result.ErrorDetail as Errors.ExceptionError)!.Exception);
                    break;
            }

            return Results.Problem(statusCode: 500, title: "Internal Server Error");
        }

        // Domain Workflow

        private async Task<Result<Payment>> RunDomainWorkflowAsync(string paymentId, CancellationToken ct)
        {
            var validationResult = validator.Validate(paymentId);
            if (!validationResult.IsValid)
            {
                return Result.Error<Payment>(new Errors.ValidationError(validationResult));
            }

            return await session.GetAsync(paymentId, ct);
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retrieve payment events")]
        partial void LogExceptionWhenRetrievingPayment(Exception exception);
    }
}
