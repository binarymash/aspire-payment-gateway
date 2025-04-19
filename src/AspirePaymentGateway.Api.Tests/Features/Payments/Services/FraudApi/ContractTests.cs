using AspirePaymentGateway.Api.Features.Payments.Services.FraudApi;
using System.Text.Json;
using static AspirePaymentGateway.Api.Features.Payments.Services.FraudApi.Contracts;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.Services.FraudApi
{
    public class ContractTests
    {
        [Fact]
        public void SerializeScreeningRequest()
        {
            var request = new ScreeningRequest
            {
                CardNumber = "1234123412341234",
                CardHolderName = "John Doe",
                ExpiryMonth = 12,
                ExpiryYear = 2023
            };
            var json = JsonSerializer.Serialize(request, FraudApiContractsContext.Default.ScreeningRequest);
            Assert.Equivalent("{\"cardNumber\":\"1234123412341234\",\"cardHolderName\":\"John Doe\",\"expiryMonth\":12,\"expiryYear\":2023}", json);
        }

        [Fact]
        public void DeserializeScreeningRequest()
        {
            var json = "{\"cardNumber\":\"4321432143214321\",\"cardHolderName\":\"John Smith\",\"expiryMonth\":11,\"expiryYear\":2029}";
            var request = JsonSerializer.Deserialize(json, FraudApiContractsContext.Default.ScreeningRequest);
            Assert.Equivalent(new ScreeningRequest
            {
                CardNumber = "4321432143214321",
                CardHolderName = "John Smith",
                ExpiryMonth = 11,
                ExpiryYear = 2029
            }, request);
        }

        [Fact]
        public void SerializeScreeningResponse()
        {
            var response = new ScreeningResponse { Accepted = true };
            var json = JsonSerializer.Serialize(response, FraudApiContractsContext.Default.ScreeningResponse);
            Assert.Equivalent("{\"accepted\":true}", json);
        }

        [Fact]
        public void DeserializeScreeningResponse()
        {
            var json = "{\"someNumber\":123,\"accepted\":true}";
            var response = JsonSerializer.Deserialize(json, FraudApiContractsContext.Default.ScreeningResponse);
            Assert.Equivalent(new ScreeningResponse { Accepted = true }, response);
        }
    }
}
