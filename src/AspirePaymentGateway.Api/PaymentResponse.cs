using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AspirePaymentGateway.Api
{
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
        string Status)
    {
    }
}
