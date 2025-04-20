namespace AspirePaymentGateway.Tests.Payments.Failures
{
    [Collection(nameof(PaymentCollection))]
    public class BadRequestTests(PaymentFixture Fixture)
    {
        [Fact]
        public async Task CreatePaymentWithNullContentReturns400()
        {
            //Arrange
            using var createPaymentRequest = Fixture.PaymentGateway.CreatePaymentRequest;

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            await Verify(createPaymentResponse).ScrubMember("traceId");
        }

        [Fact]
        public async Task CreatePaymentWithEmptyRequestReturns400()
        {
            //Arrange
            using var createPaymentRequest = Fixture.PaymentGateway.CreatePaymentRequest
                .WithContent(TestData.PaymentRequest.Empty);

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            await Verify(createPaymentResponse).ScrubMember("traceId");
        }

        [Fact]
        public async Task CreatePaymentWithMalformedJsonReturns400()
        {
            //Arrange
            using var createPaymentRequest = Fixture.PaymentGateway.CreatePaymentRequest
                .WithContent("blah");

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            await Verify(createPaymentResponse).ScrubMember("traceId");
        }

        [Fact]
        public async Task CreatePaymentWithInvalidRequestReturns400()
        {
            //Arrange
            using var createPaymentRequest = Fixture.PaymentGateway.CreatePaymentRequest
                .WithContent(TestData.PaymentRequest.Invalid);

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            await Verify(createPaymentResponse).ScrubMember("traceId");
        }
    }
}
