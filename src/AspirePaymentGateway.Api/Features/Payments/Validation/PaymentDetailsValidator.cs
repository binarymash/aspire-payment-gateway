﻿namespace AspirePaymentGateway.Api.Features.Payments.Validation
{
    using BinaryMash.Extensions.Currencies;
    using FluentValidation;
    using static AspirePaymentGateway.Api.Features.Payments.Contracts;

    public class PaymentDetailsValidator : AbstractValidator<PaymentDetails>
    {
        private const long maxAmount = 99999999;

        public PaymentDetailsValidator()
        {
            RuleFor(r => r.CurrencyCode)
                .Must(BeValidIsoCurrencyCode).WithMessage("'{PropertyName}' must be a valid ISO 3-character currency code");

            RuleFor(r => r.Amount)
                .GreaterThan(0)
                .LessThanOrEqualTo(maxAmount);
        }

        private static bool BeValidIsoCurrencyCode(string currencyCode)
        {
            return IsoCurrencyCodes.IsValid(currencyCode);
        }
    }
}
