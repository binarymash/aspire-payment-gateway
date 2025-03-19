using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspirePaymentGateway.Api
{
    public record CardExpiry(
        [property: Description("The month of expiry")]
        [property: Required]
        [property: Range(1, 12)]
        [property: DefaultValue(7)]
        int Month,

        [property: Description("The year od expiry")]
        [property: Required]
        [property: Range(2025, 2099)]
        [property: DefaultValue(2025)]
        int Year)
    {
    }
}
