namespace AspirePaymentGateway.Api
{
    using FluentValidation;

    public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator()
        {
            //RuleFor(r => r.ExpiryMonth)
            //   .GreaterThanOrEqualTo(1)
            //   .LessThanOrEqualTo(12);

            //RuleFor(r => r.ExpiryYear)
            //    .GreaterThan(DateTime.UtcNow.Year);

            RuleFor(r => r.Card.CardHolderName)
                .NotEmpty()
                .MaximumLength(35);

            RuleFor(r => r.Payment.Amount)
                .GreaterThan(0)
                .LessThanOrEqualTo(long.MaxValue);
        }
    }
}
