namespace AspirePaymentGateway.Api.Tests.Features.Payments.GetPayment.Http404NotFound
{
    [Collection(nameof(GetPaymentCollection))]
    public class BadRequestTests
    {
        GetPaymentFixture _fixture;

        public BadRequestTests(GetPaymentFixture fixture)
        {
            _fixture = fixture.Reset();
        }

        [Theory]
        [InlineData("1", "1 character")]
        [InlineData("123456789012345678901234567890123456789", "39 characters is too short")]
        [InlineData("12345678901234567890123456789012345678901", "41 characters is too long")]
        public async Task InvalidPaymentId(string paymentId, string scenario)
        {
            await Verify(await _fixture.GetPaymentHandler.GetPaymentAsync(paymentId, TestContext.Current.CancellationToken)).ScrubInlineGuids().UseParameters(scenario);
        }
    }
}
