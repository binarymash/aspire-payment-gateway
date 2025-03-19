using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AspirePaymentGateway.Api
{
    public record CardDetails(
        [property: Description("The card number of the customer")]
        [property: Required]
        [property: DefaultValue("4444333322221111")]
        [property: CreditCard]
        string CardNumber,

        [property: Description("The name of the card holder")]
        [property: Required]
        [property: Length(32, 32)]
        [property: DefaultValue("Philip Wood")]
        [property: RegularExpression(@"^[a-zA-Z]$")]
        string CardHolderName,

        [property: Description("The expiry date of the card")]
        [property: Required]
        CardExpiry Expiry,

        [property: Description("The Card Verification Value")]
        [property: Required]
        [property: Range(1, 999)]
        [property: DefaultValue(123)]
        int CVV)
    {
    }
}
