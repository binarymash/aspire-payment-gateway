using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AspirePaymentGateway.Api
{
    public record PaymentDetails(
        [property: Description("The amount in minor units")]
        [property: Required]
        [property: Range(1, long.MaxValue)]
        [property: DefaultValue(999L)]
        long Amount,

        [property: Description("ISO 3-char currency code")]
        [property: Required]
        [property: AllowedValues("USD", "GBP", "EUR")]
        [property: DefaultValue("GBP")]
        string CurrencyCode)
    {
    }
}
