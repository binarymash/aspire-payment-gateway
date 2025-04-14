namespace AspirePaymentGateway.Api.Tests.Features.Payments.GetPayment.Http404NotFound
{
    [Collection(nameof(GetPaymentCollection))]
    public class PaymentNotFoundTests
    {
        GetPaymentFixture _fixture;

        public PaymentNotFoundTests(GetPaymentFixture fixture)
        {
            _fixture = fixture.Reset();
        }

        [Fact]
        public async Task PaymentDoesNotExist()
        {
            var paymentId = $"pay_{Guid.NewGuid()}";

            await Verify(await _fixture.GetPaymentHandler.GetPaymentAsync(paymentId, TestContext.Current.CancellationToken)).ScrubInlineGuids();
        }

    }
}
