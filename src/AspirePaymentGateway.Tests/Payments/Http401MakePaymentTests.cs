using Shouldly;
using System.Net.Http.Headers;
using System.Text;

namespace AspirePaymentGateway.Tests.Payments
{
    [Collection(nameof(PaymentCollection))]
    public class Http401MakePaymentTests
    {
        private readonly PaymentFixture _fixture;

        public Http401MakePaymentTests(PaymentFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task NominalPaymentCreation()
        {
            //Arrange
            var paymentRequest = "{\"card\": {\"card_number\": \"4444333322221111\",\"card_holder_name\": \"Philip Wood\",\"expiry\": {\"month\": 7,\"year\": 2025},\"cvv\": 123},\"payment\": {\"amount\": 999,\"currency_code\": \"GBP\"}}";

            //Act
            var createPaymentResponse = await _fixture.PaymentGateway.CreatePaymentAsync(paymentRequest, TestContext.Current.CancellationToken);

            // Assert
            createPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

            //// Arrange
            //using var getPaymentMessage = new HttpRequestMessage
            //{
            //    Method = HttpMethod.Get,
            //    //RequestUri = new Uri(_fixture.PaymentGateway.BaseAddress, createPaymentResponse.Headers.Location),
            //    RequestUri = createPaymentResponse.Headers.Location,
            //};

            //getPaymentMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            //getPaymentMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var getPaymentResponse = await _fixture.PaymentGateway.GetPaymentAsync(createPaymentResponse.Headers.Location, TestContext.Current.CancellationToken);
//            var getPaymentResponse = await _fixture.PaymentGateway.SendAsync(getPaymentMessage, TestContext.Current.CancellationToken);

            // Assert
            getPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

    }
}
