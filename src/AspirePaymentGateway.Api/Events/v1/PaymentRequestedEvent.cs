using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v1
{
    public record PaymentRequestedEvent : PaymentEvent
    {
        public required long Amount { get; init; }

        public required string Currency { get; init; }

        public required string CardNumber { get; init; }

        public required string CardHolderName { get; init; }

        public required int ExpiryMonth { get; init; }

        public required int ExpiryYear { get; init; }

        public int Cvv { get; init; }

        [SetsRequiredMembers]

        public PaymentRequestedEvent(string paymentId, string occurredAt, long amount, string currency, string cardNumber, string cardHolderName, int expiryMonth, int expiryYear, int cvv)
            : base(paymentId, nameof(PaymentRequestedEvent), occurredAt)
        {
            Amount = amount;
            Currency = currency;
            CardNumber = cardNumber;
            CardHolderName = cardHolderName;
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            Cvv = cvv;
        }
        
        // for dynamo serialization
        public PaymentRequestedEvent()
        {            
        }
    }
}
