//using AspirePaymentGateway.Api.Events.v1;
//using System.Text.Json.Serialization;

//namespace AspirePaymentGateway.Api.Events
//{
//    //Required for serialization in api responses
//    [JsonDerivedType(typeof(PaymentRequestedEvent))]
//    [JsonDerivedType(typeof(PaymentAuthorisedEvent))]
//    [JsonDerivedType(typeof(PaymentDeclinedEvent))]
//    public interface IPaymentEvent
//    {
//        public string Id { get; }

//        public string Action { get; }

//        public string OccurredAt { get; }
//    }
//}