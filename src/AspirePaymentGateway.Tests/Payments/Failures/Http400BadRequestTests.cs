using Shouldly;

namespace AspirePaymentGateway.Tests.Payments.Failures
{
    [Collection(nameof(PaymentCollection))]
    public class BadRequestTests
    {
        private readonly PaymentFixture _fixture;

        public BadRequestTests(PaymentFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreatePaymentWithNullContentReturns400()
        {
            //Arrange
            using var createPaymentRequest = _fixture.PaymentGateway.CreatePaymentRequest;

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            createPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            createPaymentResponse.Headers.Location.ShouldBeNull();
            //todo: validate content
        }

        [Fact]
        public async Task CreatePaymentWithEmptyRequestReturns400()
        {
            //Arrange
            using var createPaymentRequest = _fixture.PaymentGateway.CreatePaymentRequest
                .WithContent(TestData.PaymentRequest.Empty);

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            await Verify(createPaymentResponse).ScrubMember("traceId");
        }

        [Fact]
        public async Task CreatePaymentWithMalformedJsonReturns400()
        {
            // Create payment

            //Arrange
            using var createPaymentRequest = _fixture.PaymentGateway.CreatePaymentRequest
                .WithContent("blah");

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            createPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            createPaymentResponse.Headers.Location.ShouldBeNull();
            //todo: validate content
        }

        [Fact]
        public async Task CreatePaymentWithInvalidRequestReturns400()
        {
            // Create payment

            //Arrange
            using var createPaymentRequest = _fixture.PaymentGateway.CreatePaymentRequest
                .WithContent(TestData.PaymentRequest.Invalid);

            // Act
            var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

            // Assert
            await Verify(createPaymentResponse).ScrubMember("traceId");
        }
    }
}
