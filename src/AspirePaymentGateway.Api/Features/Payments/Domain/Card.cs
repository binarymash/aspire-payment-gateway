using static AspirePaymentGateway.Api.Features.Payments.Contracts;

namespace AspirePaymentGateway.Api.Features.Payments.Domain
{
    public class Card
    {
        public required string CardNumber { get; init; }
  
        public required string CardHolderName { get; init; }

        public required CardExpiry Expiry { get; init; }

        public required int CVV { get; init; }
    }
}
