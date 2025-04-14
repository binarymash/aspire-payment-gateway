using Moq;
using Shouldly;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.Services.BankApi.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.Services.FraudApi.Contracts;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.CreatePayment.Http201Created
{
    [Collection(nameof(CreatePaymentCollection))]
    public class NominalRequestTests
    {
        public NominalRequestTests(CreatePaymentFixture fixture)
        {
            Fixture = fixture.Reset();
        }

        public CreatePaymentFixture Fixture { get; init; }

        [Fact]
        public async Task NominalRequestWhichIsAccepted()
        {
            // arrange
            PaymentRequest request = TestData.PaymentRequests.Nominal;

            ScreeningRequest? screeningRequestSentToFraudApi = null;
            ScreeningRequest expectedScreeningRequest = new()
            {
                CardHolderName = request.Card.CardHolderName,
                CardNumber = request.Card.CardNumber,
                ExpiryMonth = request.Card.Expiry.Month,
                ExpiryYear = request.Card.Expiry.Year
            };
            ScreeningResponse screeningResponseFromFraudApi = new() { Accepted = true };

            Fixture.FraudApi
                .Setup(fraud => fraud.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()))
                .Callback<ScreeningRequest, CancellationToken>((screeningRequest, ct) => screeningRequestSentToFraudApi = screeningRequest)
                .ReturnsAsync(screeningResponseFromFraudApi);

            AuthorisationRequest? authorisationRequestSentToBankApi = null;
            AuthorisationRequest expectedAuthorisationRequest = new();
            AuthorisationResponse authorisationResponseFromBankApi = new()
            { 
                AuthorisationRequestId = Guid.NewGuid().ToString(),
                Authorised = true,
                AuthorisationCode = "ABCDEF",
            };

            Fixture.BankApi
                .Setup(bank => bank.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()))
                .Callback<AuthorisationRequest, CancellationToken>((authorisationRequest, ct) => authorisationRequestSentToBankApi = authorisationRequest)
                .ReturnsAsync(authorisationResponseFromBankApi);

            // act
            var verify = Verify(await Fixture.CreatePaymentHandler.PostPaymentAsync(request, default)).ScrubInlineGuids();

            // assert
            Fixture.FraudApi.Verify(api => api.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            screeningRequestSentToFraudApi.ShouldBeEquivalentTo(expectedScreeningRequest);

            Fixture.BankApi.Verify(api => api.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            authorisationRequestSentToBankApi.ShouldBeEquivalentTo(expectedAuthorisationRequest);

            var paymentFateInstrument = Fixture.PaymentFateCountCollector.GetMeasurementSnapshot();
            paymentFateInstrument.Count.ShouldBe(1);

            var paymentRequestedInstrument = Fixture.PaymentRequestedCountCollector.GetMeasurementSnapshot();
            paymentRequestedInstrument.Count.ShouldBe(1);

            await verify;
        }

        [Fact]
        public async Task NominalRequestWhichIsDeclinedByBank()
        {
            PaymentRequest request = TestData.PaymentRequests.Nominal;

            ScreeningRequest? screeningRequestSentToFraudApi = null;
            ScreeningRequest expectedScreeningRequest = new()
            {
                CardHolderName = request.Card.CardHolderName,
                CardNumber = request.Card.CardNumber,
                ExpiryMonth = request.Card.Expiry.Month,
                ExpiryYear = request.Card.Expiry.Year
            };
            ScreeningResponse screeningResponseFromFraudApi = new() { Accepted = true };

            Fixture.FraudApi
                .Setup(fraud => fraud.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()))
                .Callback<ScreeningRequest, CancellationToken>((screeningRequest, ct) => screeningRequestSentToFraudApi = screeningRequest)
                .ReturnsAsync(screeningResponseFromFraudApi);

            AuthorisationRequest? authorisationRequestSentToBankApi = null;
            AuthorisationRequest expectedAuthorisationRequest = new();
            AuthorisationResponse authorisationResponseFromBankApi = new()
            {
                AuthorisationRequestId = Guid.NewGuid().ToString(),
                Authorised = false,
                AuthorisationCode = "GHIJK",
            };

            Fixture.BankApi
                .Setup(bank => bank.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()))
                .Callback<AuthorisationRequest, CancellationToken>((authorisationRequest, ct) => authorisationRequestSentToBankApi = authorisationRequest)
                .ReturnsAsync(authorisationResponseFromBankApi);

            var verify = Verify(await Fixture.CreatePaymentHandler.PostPaymentAsync(request, default)).ScrubInlineGuids();

            Fixture.FraudApi.Verify(api => api.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            screeningRequestSentToFraudApi.ShouldBeEquivalentTo(expectedScreeningRequest);

            Fixture.BankApi.Verify(api => api.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            authorisationRequestSentToBankApi.ShouldBeEquivalentTo(expectedAuthorisationRequest);

            var paymentFateInstrument = Fixture.PaymentFateCountCollector.GetMeasurementSnapshot();
            paymentFateInstrument.Count.ShouldBe(1);

            var paymentRequestedInstrument = Fixture.PaymentRequestedCountCollector.GetMeasurementSnapshot();
            paymentRequestedInstrument.Count.ShouldBe(1);

            await verify;
        }
    }
}
