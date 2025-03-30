using Moq;
using Shouldly;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.BankApi.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi.Contracts;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.CreatePayment
{
    public class BadRequestTests : ComponentTests
    {
        [Fact]
        public Task NullRequest()
        {
            return VerifyPaymentRequestIsRejected(null!);
        }

        [Fact]
        public Task InvalidRequest()
        {
            return VerifyPaymentRequestIsRejected(new PaymentRequest(null!, null!));
        }

        [Fact]
        public Task EmptyCard()
        {
            PaymentRequest request = NominalRequest with
            {
                Card = new(null!, null!, null!, 0)
            };

            return VerifyPaymentRequestIsRejected(request);
                
        }

        [Theory]
        [InlineData(null, "Null")]
        [InlineData("", "Empty")]
        [InlineData(" ", "Whitespace")]
        [InlineData("12345678901", "11Characters")]
        [InlineData("12345678901234567890", "20Characters")]
        [InlineData("abcdefghijklmnop", "Text")]
        [InlineData("abcde67890123456", "TextAndNumbers")]
        public Task InvalidCardNumber(string? cardNumber, string scenario)
        {
            PaymentRequest request = NominalRequest with
            {
                Card = NominalRequest.Card with
                {
                    CardNumber = cardNumber!
                }
            };

            return VerifyPaymentRequestIsRejected(request, scenario);
        }

        [Theory]
        [InlineData(0, "0")]
        [InlineData(1000, "1000")]
        public Task InvalidCardCvv(int cvv, string scenario)
        {
            PaymentRequest request = NominalRequest with
            {
                Card = NominalRequest.Card with
                {
                    CVV = cvv
                }
            };

            return VerifyPaymentRequestIsRejected(request, scenario);
        }

        [Theory]
        [InlineData(null, "Null")]
        [InlineData("", "Empty")]
        [InlineData(" ", "Whitespace")]
        [InlineData("a", "1Character")]
        [InlineData("Dave Smith 2345678901234567890123456", "36Characters")]
        [InlineData("1234 5678", "AllDigits")]
        public Task InvalidCardholderName(string? cardholderName, string scenario)
        {
            PaymentRequest request = NominalRequest with
            {
                Card = NominalRequest.Card with
                {
                    CardHolderName = cardholderName!
                }
            };

            return VerifyPaymentRequestIsRejected(request, scenario);
        }

        [Fact]
        public Task EmptyPayment()
        {
            PaymentRequest request = NominalRequest with
            {
                Payment = new(0, null!)
            };

            return VerifyPaymentRequestIsRejected(request);
        }

        private async Task VerifyPaymentRequestIsRejected(PaymentRequest request, string? scenario = null)
        {
            var verify = Verify(await _handler.PostPaymentAsync(request, default));
            if (scenario!= null)
            {
                verify.UseParameters(scenario);
            }

            _fraudApi.Verify(api => api.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            _bankApi.Verify(api => api.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()), Times.Never);

            var paymentFateInstrument = _paymentFateCountCollector.GetMeasurementSnapshot();
            paymentFateInstrument.Count.ShouldBe(0);

            var paymentRequestedInstrument = _paymentRequestedCountCollector.GetMeasurementSnapshot();
            paymentRequestedInstrument.Count.ShouldBe(1);

            await verify;
        }
    }
}
