namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.Validation
{
    using FluentValidation;
    using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.Contracts;

    public class CardDetailsValidator : AbstractValidator<CardDetails>
    {
        private static string NameRegex = @"^[a-z'\- A-Z\s]*$";

        public CardDetailsValidator()
        {
            RuleFor(r => r.CardHolderName)
                .NotNull()
                .MaximumLength(35)
                .Matches(NameRegex);

            RuleFor(r => r.CVV)
                .NotNull()
                .ExclusiveBetween(1, 999);

            RuleFor(r => r.CardNumber)
                .NotNull()
                .Matches(@"^\d{12,19}$")
                .Must(BeValidLuhn);

            RuleFor(r => r.Expiry)
                .NotNull()
                .SetValidator(new CardExpiryValidator());
        }

        private static bool BeValidLuhn(string cardNumber)
        {
            return true;
        }
    }
}
