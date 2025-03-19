using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AspirePaymentGateway.Api
{
    public record CardDetails
    {
        [Description("The card number of the customer")]
        [Required]
        [DefaultValue("4444333322221111")]
        [CreditCard]
        public required string CardNumber { get; init; }

        [Description("The name of the card holder")]
        //[Required]
        //[Length(32, 32)]
        [DefaultValue("Philip Wood")]
        //[RegularExpression(@"^[a-zA-Z]$")]
        public required string CardHolderName { get; init; }

        [Description("The expiry date of the card")]
        [Required]
        public required CardExpiry Expiry { get; init; }

        [Description("The Card Verification Value")]
        [Required]
        [Range(1, 999)]
        [DefaultValue(123)]
        public int CVV { get; init; }
    }
}
