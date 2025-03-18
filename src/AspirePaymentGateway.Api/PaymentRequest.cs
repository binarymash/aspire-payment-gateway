using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspirePaymentGateway.Api
{
    public record PaymentRequest
    {
        [Description("The details of the card being used")]
        [Required]
        public required CardDetails Card { get; init; }

        [Description("The details of the payment")]
        [Required]
        public required PaymentDetails Payment { get; init; }


    }
}
