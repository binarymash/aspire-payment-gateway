using FluentResults;
using FluentValidation.Results;

namespace AspirePaymentGateway.Api.Extensions.Results
{
    public static partial class Errors
    {
        public static ValidationError ValidationError(ValidationResult validationResult) => new ValidationError(validationResult);
    }

    public class ValidationError : Error
    {
        public ValidationError(ValidationResult validationResult)
        {
            ValidationResult = validationResult;
        }

        public ValidationResult ValidationResult { get; set; }
    }
}
