using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AspirePaymentGateway.Api
{
    public record PaymentDetails
    {
        [Description("The amount in minor units")]
        [Required]
        [Range(1, long.MaxValue)]
        [DefaultValue(999L)]
        public long Amount { get; init; }

        [Description("ISO 3-char currency code")]
        [Required]
        [AllowedValues("USD", "GBP", "EUR")]
        [DefaultValue("GBP")]
        public required string CurrencyCode { get; init; }
    }
}
