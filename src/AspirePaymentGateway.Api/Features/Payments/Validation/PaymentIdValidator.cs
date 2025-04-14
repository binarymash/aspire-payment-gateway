using FluentValidation;

namespace AspirePaymentGateway.Api.Features.Payments.Validation
{
    public class PaymentIdValidator : AbstractValidator<string>
    {
        public PaymentIdValidator()
        {
            RuleFor(paymentId => paymentId)
                .NotEmpty()
                .Length(40).WithMessage("Payment ID must be 40 characters in length.");
        }
    }
}
