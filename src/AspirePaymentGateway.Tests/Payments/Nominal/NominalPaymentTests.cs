using Shouldly;

namespace AspirePaymentGateway.Tests.Payments.Nominal
{
    [Collection(nameof(PaymentCollection))]
    public class NominalPaymentTests
    {
        private readonly PaymentFixture _fixture;

        public NominalPaymentTests(PaymentFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PaymentCanBeCreatedAndRetrieved()
        {
            // Create payment

            //Arrange
            using var createPaymentRequest = _fixture.PaymentGateway.CreatePaymentRequest
                .WithContent(TestData.PaymentRequest.Nominal);

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            createPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

            // Get payment

            //Arrange
            using var getPaymentRequest = _fixture.PaymentGateway.GetPaymentRequest
                .WithLocation(createPaymentResponse.Headers.Location);

            // Act
            var getPaymentResponse = await getPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            getPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }   
    }
}
