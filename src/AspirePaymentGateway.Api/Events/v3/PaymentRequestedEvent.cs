using Amazon.DynamoDBv2.DataModel;
using AspirePaymentGateway.Api.Storage.DynamoDb;
using System.Diagnostics.CodeAnalysis;

namespace AspirePaymentGateway.Api.Events.v3
{
    public record PaymentRequestedEvent(
        string Id, 
        string OccurredAt, 
        long Amount, 
        string Currency, 
        string CardNumber, 
        string CardHolderName, 
        int ExpiryMonth, 
        int ExpiryYear, 
        int Cvv)
        : PaymentEvent(Id, nameof(PaymentRequestedEvent), OccurredAt)
    {        
        // for dynamo serialization
        public PaymentRequestedEvent() : this("", "", 0, "", "", "", 0, 0, 0)
        {
        }
    }
}
