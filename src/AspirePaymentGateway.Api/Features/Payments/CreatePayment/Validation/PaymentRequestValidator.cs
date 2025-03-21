namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.Validation
{
    using FluentValidation;
    using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.Contracts;

    public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator()
        {
            RuleFor(r => r.Card)
                .NotNull()
                .SetValidator(new CardDetailsValidator());

            RuleFor(r => r.Payment)
                .NotNull()
                .SetValidator(new PaymentDetailsValidator());
        }
    }
}
