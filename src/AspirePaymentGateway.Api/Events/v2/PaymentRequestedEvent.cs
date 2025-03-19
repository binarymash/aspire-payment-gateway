using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v2
{
    public record PaymentRequestedEvent : PaymentEvent
    {
        public PaymentRequestedEvent()
        {
            Action = nameof(PaymentRequestedEvent);
        }

        public required long Amount { get; init; }

        public required string Currency { get; init; }

        public required string CardNumber { get; init; }

        public required string CardHolderName { get; init; }

        public required int ExpiryMonth { get; init; }

        public required int ExpiryYear { get; init; }

        public required int Cvv { get; init; }
    }
}
