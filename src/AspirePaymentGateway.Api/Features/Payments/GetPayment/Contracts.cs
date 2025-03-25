using AspirePaymentGateway.Api.Features.Payments.Events;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspirePaymentGateway.Api.Features.Payments.GetPayment
{
    // The annotations are used to generate the OpenAPI spec
    public static class Contracts
    {
        public record GetPaymentResponse(
            [Description("The events that have occurred on the payment")]
            [Required]
            IEnumerable<IPaymentEvent> Events);
    }
}
