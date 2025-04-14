using Shouldly;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.GetPayment.Http404NotFound
{
    public class BadRequestTests : ComponentTests
    {
        [Theory]
        [InlineData("1", "1 character")]
        [InlineData("123456789012345678901234567890123456789", "39 characters is too short")]
        [InlineData("12345678901234567890123456789012345678901", "41 characters is too long")]
        public async Task InvalidPaymentId(string paymentId, string scenario)
        {
            var verify = Verify(await GetPaymentHandler.GetPaymentAsync(paymentId, default)).ScrubInlineGuids().UseParameters(scenario);


            FraudApi.VerifyNoOtherCalls();
            BankApi.VerifyNoOtherCalls();

            var paymentFateInstrument = PaymentFateCountCollector.GetMeasurementSnapshot();
            paymentFateInstrument.Count.ShouldBe(0);

            var paymentRequestedInstrument = PaymentRequestedCountCollector.GetMeasurementSnapshot();
            paymentRequestedInstrument.Count.ShouldBe(0);

            await verify;
        }

    }
}
