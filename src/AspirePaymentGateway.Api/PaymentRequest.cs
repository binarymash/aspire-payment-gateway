using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspirePaymentGateway.Api
{
    public record PaymentRequest(
        [property: Description("The details of the card being used")]
        [property: Required]
        CardDetails Card,

        [property: Description("The details of the payment")]
        [property: Required]
        PaymentDetails Payment)
    {
    }
}
