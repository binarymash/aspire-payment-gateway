using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspirePaymentGateway.Api
{
    public record CardExpiry()
    {

        [Description("The month of expiry")]
        [Required]
        [Range(1, 12)]
        [DefaultValue(7)]
        public int Month { get; init; }

        [Description("The year od expiry")]
        [Required]
        [Range(2025, 2099)]
        [DefaultValue(2025)]
        public int Year { get; init; }

    }
}
