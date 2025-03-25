namespace AspirePaymentGateway.Api.Features.Payments.Events
{
    public class Payment
    {
        public string Id { get; private set; }

        public decimal Amount { get; private set; }
        
        public string CurrencyCode { get; private set; }
        
        public string CardNumber { get; private set; }
        
        public string CardExpiry { get; private set; }
        
        public string CardSecurityCode { get; private set; }
        
        public string CardHolderName { get; private set; }
        
        public string Status { get; private set; }

        public string? DeclineReason { get; private set; }
        
        public string AuthorisationCode { get; private set; }

        private List<IPaymentEvent> paymentEvents = new List<IPaymentEvent>();

        public void Apply(PaymentRequestedEvent @event)
        {
            paymentEvents.Add(@event);
        }

        public void Apply(PaymentAuthorisedEvent @event)
        {
            paymentEvents.Add(@event);
            AuthorisationCode = @event.AuthorisationCode;
        }

        public void Apply(PaymentDeclinedEvent @event)
        {
            paymentEvents.Add(@event);
            Status = Events.PaymentStatus.Declined;
            DeclineReason = @event.Reason;
        }
    }
}
