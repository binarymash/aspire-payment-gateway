using AspirePaymentGateway.Api.Features.Payments.Domain;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.GetPayment.Http200Ok
{
    [Collection(nameof(GetPaymentCollection))]
    public class GetPaymentThatExistsTests
    {
        GetPaymentFixture _fixture;

        public GetPaymentThatExistsTests(GetPaymentFixture fixture)
        {
            _fixture = fixture.Reset();
        }

        [Fact]
        public async Task GetAcceptedPayment()
        {
            // Arrange
            string paymentId = $"pay_{Guid.NewGuid().ToString()}";

            var payment = TestData.Payment.AcceptedByBank(paymentId);

            await _fixture.Repository.SaveAsync(payment.UncommittedEvents, TestContext.Current.CancellationToken);

            // Act
            var response = await _fixture.GetPaymentHandler.GetPaymentAsync(paymentId, TestContext.Current.CancellationToken);

            // Assert
            await Verify(response).ScrubInlineGuids();
        }

        [Fact]
        public async Task GetPaymentDeclinedByBank()
        {
            // Arrange
            string paymentId = $"pay_{Guid.NewGuid().ToString()}";

            var payment = TestData.Payment.DeclinedByBank(paymentId);

            await _fixture.Repository.SaveAsync(payment.UncommittedEvents, TestContext.Current.CancellationToken);

            // Act
            var response = await _fixture.GetPaymentHandler.GetPaymentAsync(paymentId, TestContext.Current.CancellationToken);

            // Assert
            await Verify(response).ScrubInlineGuids();
        }
    }
}
