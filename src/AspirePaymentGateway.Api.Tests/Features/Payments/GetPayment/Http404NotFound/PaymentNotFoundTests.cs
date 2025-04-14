using Shouldly;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.GetPayment.Http404NotFound
{
    public class PaymentNotFoundTests : ComponentTests
    {
        [Fact]
        public async Task PaymentDoesNotExist()
        {
            var paymentId = $"pay_{Guid.NewGuid()}";

            var verify = Verify(await GetPaymentHandler.GetPaymentAsync(paymentId, default)).ScrubInlineGuids();

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
