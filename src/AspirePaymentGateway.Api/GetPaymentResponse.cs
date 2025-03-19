using AspirePaymentGateway.Api.Events;
using AspirePaymentGateway.Api.Events.v4;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspirePaymentGateway.Api
{
    public record GetPaymentResponse(
        [Description("The events that have occurred on the payment")]
        [Required]
        IEnumerable<IPaymentEvent> Events)
    {
    }
}
