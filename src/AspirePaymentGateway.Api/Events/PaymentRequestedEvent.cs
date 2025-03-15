using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;

namespace AspirePaymentGateway.Api.Events
{
    [DynamoDBTable(Constants.TableName)]
    public record PaymentRequestedEvent : PaymentEvent
    {
        public PaymentRequestedEvent(string paymentId) : base(paymentId, nameof(PaymentRequestedEvent))
        {            
        }

        public PaymentRequestedEvent()
        {
            
        }

        public required long Amount { get; init; }

        public required string Currency { get; init; }

        public required string CardNumber { get; init; }

        public required string CardHolderName {get; init; }

        public required int ExpiryMonth { get; init; }

        public required int ExpiryYear { get; init; }

        public required int Cvv { get; init; }
    }
}
