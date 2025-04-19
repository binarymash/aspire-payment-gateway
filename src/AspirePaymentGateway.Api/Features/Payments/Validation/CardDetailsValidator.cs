namespace AspirePaymentGateway.Api.Features.Payments.Validation
{
    using FluentValidation;
    using static AspirePaymentGateway.Api.Features.Payments.Contracts;

    public class CardDetailsValidator : AbstractValidator<CardDetails>
    {
        private const string NameRegex = @"^[a-z'\- A-Z\s]*$";

        public CardDetailsValidator()
        {
            RuleFor(r => r.CardHolderName)
                .NotEmpty()
                .Length(3, 35)
                .Matches(NameRegex);

            RuleFor(r => r.CVV)
                .ExclusiveBetween(100, 999);

            RuleFor(r => r.CardNumber)
                .NotEmpty()
                .Length(12, 19)
                .Matches(@"^\d*$").WithMessage("'{PropertyName}' must be all digits.")
                .Must(BeValidLuhn);

            RuleFor(r => r.Expiry)
                .NotEmpty()
                .SetValidator(new CardExpiryValidator());
        }

        private static bool BeValidLuhn(string cardNumber)
        {
            return true;
        }
    }
}
