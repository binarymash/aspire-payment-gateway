using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AspirePaymentGateway.Api
{
    public record PaymentResponse
    {
        [Description("The ID of the payment")]
        [Required]
        [DefaultValue("pay_1ec62e16-5403-4a93-bd31-5804daec4263")]
        public required string PaymentId { get; init; }

        [Description("The status of the payment")]
        [Required]
        [AllowedValues(PaymentStatus.Pending, PaymentStatus.Authorised, PaymentStatus.Declined)]
        [DefaultValue(PaymentStatus.Authorised)]
        public required string Status { get; init; }
    }
}
