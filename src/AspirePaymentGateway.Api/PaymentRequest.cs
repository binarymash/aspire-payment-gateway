namespace AspirePaymentGateway.Api
{
    public class PaymentRequest
    {
        public string CardNumber { get; init; }

        public string CardHolderName { get; init; }
        
        public int ExpiryMonth { get; init; }
        
        public int ExpiryYear { get; init; }
        
        public int CVV { get; init; }
        
        public long Amount { get; init; }

        public string RequestId { get; init; }

        public string CurrencyCode { get; init; }
    }
}
