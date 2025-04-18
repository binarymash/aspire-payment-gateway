using Shouldly;

namespace AspirePaymentGateway.Tests.Payments.Nominal
{
    [Collection(nameof(PaymentCollection))]
    public class NominalPaymentTest(PaymentFixture Fixture)
    {
        [Fact]
        public async Task PaymentCanBeCreatedAndRetrieved()
        {
            // Create payment

            //Arrange
            using var createPaymentRequest = Fixture.PaymentGateway.CreatePaymentRequest
                .WithContent(TestData.PaymentRequest.Nominal);

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            createPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

            // Get payment

            //Arrange
            using var getPaymentRequest = Fixture.PaymentGateway.GetPaymentRequest
                .WithLocation(createPaymentResponse.Headers.Location);

            // Act
            var getPaymentResponse = await getPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            getPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
