using Moq;
using Shouldly;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.Services.BankApi.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.Services.FraudApi.Contracts;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.CreatePayment.Http400BadRequests
{
    [Collection(nameof(CreatePaymentCollection))]
    public class BadRequestTests
    {
        public CreatePaymentFixture Fixture { get; init; }

        public BadRequestTests(CreatePaymentFixture fixture)
        {
            Fixture = fixture.Reset();
        }

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
            PaymentRequest request = TestData.PaymentRequests.Nominal with
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
            PaymentRequest request = TestData.PaymentRequests.Nominal with
            {
                Card = TestData.PaymentRequests.Nominal.Card with
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
            PaymentRequest request = TestData.PaymentRequests.Nominal with
            {
                Card = TestData.PaymentRequests.Nominal.Card with
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
            PaymentRequest request = TestData.PaymentRequests.Nominal with
            {
                Card = TestData.PaymentRequests.Nominal.Card with
                {
                    CardHolderName = cardholderName!
                }
            };

            return VerifyPaymentRequestIsRejected(request, scenario);
        }

        [Theory]
        [InlineData(null, "GBP", "Null Amount")]
        [InlineData(-1, "GBP", "Negative Amount")]
        [InlineData(0, "GBP", "Zero Amount")]
        [InlineData(100000000, "GBP", "Amount Greater Than 99999999")]
        [InlineData(100, null, "Null Currency Code")]
        [InlineData(100, "", "Empty Currency Code")]
        [InlineData(100, " ", "Whitespace Currency Code")]
        [InlineData(100, "GBPX", "Unsupported Currency Account")]
        public Task InvalidAmount(long amount, string currencyCode, string scenario)
        {
            PaymentRequest request = TestData.PaymentRequests.Nominal with
            {
                Payment = new(amount, currencyCode)
            };

            return VerifyPaymentRequestIsRejected(request, scenario);
        }

        [Fact]
        public Task EmptyPayment()
        {
            PaymentRequest request = TestData.PaymentRequests.Nominal with
            {
                Payment = new(0, null!)
            };

            return VerifyPaymentRequestIsRejected(request);
        }

        private async Task VerifyPaymentRequestIsRejected(PaymentRequest request, string? scenario = null)
        {
            var verify = Verify(await Fixture.CreatePaymentHandler.PostPaymentAsync(request, default));
            if (scenario!= null)
            {
                await verify.UseParameters(scenario);
            }

            Fixture.FraudApi.Verify(api => api.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            Fixture.BankApi.Verify(api => api.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()), Times.Never);

            var paymentFateInstrument = Fixture.PaymentFateCountCollector.GetMeasurementSnapshot();
            paymentFateInstrument.Count.ShouldBe(0);

            var paymentRequestedInstrument = Fixture.PaymentRequestedCountCollector.GetMeasurementSnapshot();
            paymentRequestedInstrument.Count.ShouldBe(1);
        }
    }
}
