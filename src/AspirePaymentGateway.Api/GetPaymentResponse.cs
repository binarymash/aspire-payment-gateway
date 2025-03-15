using AspirePaymentGateway.Api.Events;

namespace AspirePaymentGateway.Api
{
    public record GetPaymentResponse
    {
        public required IEnumerable<IPaymentEvent> Events { get; init; }
    }
}
