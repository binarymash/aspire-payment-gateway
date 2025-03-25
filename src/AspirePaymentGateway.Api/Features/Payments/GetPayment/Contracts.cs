using AspirePaymentGateway.Api.Features.Payments.Events;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AspirePaymentGateway.Api.Features.Payments.GetPayment
{
    // The annotations are used to generate the OpenAPI spec
    public class Contracts
    {
        public record GetPaymentResponse(
            [Description("The events that have occurred on the payment")]
            [Required]
            IEnumerable<IPaymentEvent> Events);
    }
}
