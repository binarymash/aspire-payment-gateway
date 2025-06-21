using System.Globalization;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;

namespace AspirePaymentGateway.Api.Features.Payments.Domain
{
    public class Payment
    {
        public string Id { get; private set; } = default!;

        public Card Card { get; private set; } = default!;

        public Amount Amount { get; private set; } = default!;

        public string Status { get; private set; } = PaymentStatus.None;

        public ScreeningStatus ScreeningStatus { get; private set; } = ScreeningStatus.Unscreened;

        public string? DeclineReason { get; private set; }

        public DateTime LastUpdated { get; private set; } = default!;

        public List<PaymentEvent> UncommittedEvents { get; } = [];

        public static Payment Create(
            string paymentId,
            long amountInMinorUnits,
            string currencyIsoCode,
            string cardNumber,
            string cardHolderName,
            int cvv,
            int expiryMonth,
            int expiryYear)
        {
            Payment payment = new();

            payment.Apply(new PaymentRequestedEvent()
            {
                Id = paymentId,
                OccurredAt = DateTime.UtcNow.ToString("O"),
                Amount = amountInMinorUnits,
                Currency = currencyIsoCode,
                CardNumber = cardNumber,
                CardHolderName = cardHolderName,
                Cvv = cvv,
                ExpiryMonth = expiryMonth,
                ExpiryYear = expiryYear
            });

            return payment;
        }

        public void RecordScreeningResponse(bool accepted)
        {
            Apply(new PaymentScreenedEvent
            {
                Id = this.Id,
                OccurredAt = DateTime.UtcNow.ToString("O"),
                ScreeningAccepted = accepted
            });
        }

        internal void RecordAuthorisationResponse(string authorisationRequestId, bool isAuthorised, string? authorisationCode)
        {
            Apply(new PaymentAuthorisedEvent()
            {
                Id = this.Id,
                OccurredAt = DateTime.UtcNow.ToString("O"),

                AuthorisationTraceId = authorisationRequestId,
                IsAuthorised = isAuthorised,
                AuthorisationCode = authorisationCode,
            });
        }

        public void Apply(PaymentEvent @event)
        {
            switch (@event)
            {
                case PaymentRequestedEvent paymentRequestedEvent:
                    Apply(paymentRequestedEvent);
                    break;
                case PaymentScreenedEvent paymentScreenedEvent:
                    Apply(paymentScreenedEvent);
                    break;
                case PaymentAuthorisedEvent paymentAuthorisedEvent:
                    Apply(paymentAuthorisedEvent);
                    break;
                case PaymentDeclinedEvent paymentDeclinedEvent:
                    Apply(paymentDeclinedEvent);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown event type: {@event.GetType()}");
            }
        }

        public void FlushUncommittedEvents()
        {
            UncommittedEvents.Clear();
        }

        private void Apply(PaymentRequestedEvent @event)
        {
            if (Status != PaymentStatus.None)
            {
                throw new InvalidOperationException("Payment is not in a valid state to be authorised.");
            }

            Id = @event.Id;

            Card = new()
            {
                CardNumber = @event.CardNumber,
                CardHolderName = @event.CardHolderName,
                Expiry = new CardExpiry
                {
                    Month = @event.ExpiryMonth,
                    Year = @event.ExpiryYear
                },
                CVV = @event.Cvv
            };

            Amount = new()
            {
                ValueInMinorUnits = @event.Amount,
                CurrencyCode = @event.Currency
            };

            Status = PaymentStatus.Pending;

            LastUpdated = DateTime.Parse(@event.OccurredAt, CultureInfo.InvariantCulture);
            UncommittedEvents.Add(@event);
        }

        private void Apply(PaymentScreenedEvent @event)
        {
            if (Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("Payment is not in a valid state to be screened.");
            }
            ScreeningStatus = @event.ScreeningAccepted ? ScreeningStatus.Passed : ScreeningStatus.Rejected;
            Status = PaymentStatus.Screened;

            LastUpdated = DateTime.Parse(@event.OccurredAt, CultureInfo.InvariantCulture);
            UncommittedEvents.Add(@event);
        }

        private void Apply(PaymentAuthorisedEvent @event)
        {
            if (Status != PaymentStatus.Screened)
            {
                throw new InvalidOperationException("Payment is not in a valid state to be authorised.");
            }

            Status = PaymentStatus.Authorised;

            LastUpdated = DateTime.Parse(@event.OccurredAt, CultureInfo.InvariantCulture);
            UncommittedEvents.Add(@event);
        }

        private void Apply(PaymentDeclinedEvent @event)
        {
            if (Status == PaymentStatus.Authorised)
            {
                throw new InvalidOperationException("Payment is not in a valid state to be authorised.");
            }

            Status = PaymentStatus.Declined;
            DeclineReason = @event.Reason;

            LastUpdated = DateTime.Parse(@event.OccurredAt, CultureInfo.InvariantCulture);
            UncommittedEvents.Add(@event);
        }
    }
}
