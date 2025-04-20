namespace AspirePaymentGateway.Tests.Payments.Failures
{
    [Collection(nameof(PaymentCollection))]
    public class Http500InternalServerErrorTests(PaymentFixture Fixture)
    {
        [Fact]
        public async Task CreatePaymentWithExceptionInFraudApiReturns500()
        {
            //Arrange
            using var createPaymentRequest = Fixture.PaymentGateway.CreatePaymentRequest
                .WithContent(TestData.PaymentRequest.ThrowsExceptionInFraudApi);

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            await Verify(createPaymentResponse).ScrubMember("traceId");
        }
    }
}
