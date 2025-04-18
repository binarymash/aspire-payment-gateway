using Shouldly;

namespace AspirePaymentGateway.Tests.Payments.Failures
{
    [Collection(nameof(PaymentCollection))]
    public class Http500InternalServerErrorTests
    {
        private readonly PaymentFixture _fixture;

        public Http500InternalServerErrorTests(PaymentFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreatePaymentWithExceptionInFraudApiReturns500()
        {
            //Arrange
            using var createPaymentRequest = _fixture.PaymentGateway.CreatePaymentRequest
                .WithContent(TestData.PaymentRequest.ThrowsExceptionInFraudApi);

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            createPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            createPaymentResponse.Headers.Location.ShouldBeNull();
            //todo: validate content
        }

    }
}
