namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.Validation
{
    using FluentValidation;
    using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.Contracts;

    public class CardExpiryValidator : AbstractValidator<CardExpiry>
    {
        public CardExpiryValidator()
        {
            RuleFor(r => r.Year)
                .NotNull()
                .GreaterThanOrEqualTo(DateTime.UtcNow.Year);

            RuleFor(r => r.Month)
                .NotNull()
                .InclusiveBetween(1, 12);

            RuleFor(r => r)
                .Must(IsInTheFuture).WithMessage("Card expiry must not be in the past.");
        }

        private static bool IsInTheFuture(CardExpiry cardExpiry)
        {
            var now = DateTime.UtcNow;
            return cardExpiry.Year > now.Year || (cardExpiry.Year == now.Year && cardExpiry.Month > now.Month);
        }
    }
}
