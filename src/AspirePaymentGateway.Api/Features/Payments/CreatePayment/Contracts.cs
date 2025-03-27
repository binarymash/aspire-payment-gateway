using AspirePaymentGateway.Api.Extensions.Redaction;
using AspirePaymentGateway.Api.Features.Payments.Events;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment
{
    // The annotations are used to generate the OpenAPI spec
    public static class Contracts
    {
        // request

        public record PaymentRequest(
            [property: Description("The details of the card being used")]
            [property: Required]
            [property: LogProperties]
            CardDetails Card,

            [property: Description("The details of the payment")]
            [property: Required]
            [property: LogProperties]
            PaymentDetails Payment);

        public record CardDetails(
            [property: Description("The card number of the customer")]
            [property: Required]
            [property: DefaultValue("4444333322221111")]
            [property: CreditCard]
            [property: LogProperties]
            [property: SensitiveData]
            string CardNumber,

            [property: Description("The name of the card holder")]
            [property: Required]
            [property: Length(32, 32)]
            [property: DefaultValue("Philip Wood")]
            [property: RegularExpression(@"^[a-zA-Z]$")]
            [property: LogProperties]
            [property: PiiData]
            string CardHolderName,

            [property: Description("The expiry date of the card")]
            [property: Required]
            [property: LogProperties]
            CardExpiry Expiry,

            [property: Description("The Card Verification Value")]
            [property: Required]
            [property: Range(1, 999)]
            [property: DefaultValue(123)]
            [property: LogProperties]
            int CVV);

        public record PaymentDetails(
            [property: Description("The amount in minor units")]
            [property: Required]
            [property: Range(1, long.MaxValue)]
            [property: DefaultValue(999L)]
            [property: LogProperties]
            long Amount,

            [property: Description("ISO 3-char currency code")]
            [property: Required]
            [property: AllowedValues("USD", "GBP", "EUR")]
            [property: DefaultValue("GBP")]
            [property: LogProperties]
            string CurrencyCode);

        public record CardExpiry(
            [property: Description("The month of expiry")]
            [property: Required]
            [property: Range(1, 12)]
            [property: DefaultValue(7)]
            [property: LogProperties]
            int Month,

            [property: Description("The year od expiry")]
            [property: Required]
            [property: Range(2025, 2099)]
            [property: DefaultValue(2025)]
            [property: LogProperties]
            int Year);

        // response

        [Description("Details of the payment")]
        public record PaymentResponse(
            [property: Description("The ID of the payment")]
            [property: Required]
            [property: DefaultValue("pay_1ec62e16-5403-4a93-bd31-5804daec4263")]
            string PaymentId,

            [property: Description("The status of the payment")]
            [property: Required]
            [property: AllowedValues(PaymentStatus.Pending, PaymentStatus.Authorised, PaymentStatus.Declined)]
            [property: DefaultValue(PaymentStatus.Authorised)]
            string Status);
    }
}
