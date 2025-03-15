namespace AspirePaymentGateway.Api.Events
{
    public record PaymentAuthorisedEvent : PaymentEvent
    {
        public PaymentAuthorisedEvent(string paymentId) : base(paymentId, nameof(PaymentAuthorisedEvent))
        {            
        }

        public PaymentAuthorisedEvent()
        {            
        }

        public string AuthorisationCode { get; init; }
    }
}
