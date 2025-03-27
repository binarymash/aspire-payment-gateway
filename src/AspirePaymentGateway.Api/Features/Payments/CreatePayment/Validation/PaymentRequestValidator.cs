namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.Validation
{
    using FluentValidation;

    public class PaymentRequestValidator : AbstractValidator<Contracts.PaymentRequest>
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
