using Moq;
using Shouldly;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.BankApi.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.Contracts;
using static AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi.Contracts;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.CreatePayment._201Created
{
    public class NominalRequestTests : ComponentTests
    {
        [Fact]
        public async Task NominalRequestWhichIsAccepted()
        {
            PaymentRequest request = NominalRequest;

            ScreeningRequest? screeningRequestSentToFraudApi = null;
            ScreeningRequest expectedScreeningRequest = new()
            {
                CardHolderName = request.Card.CardHolderName,
                CardNumber = request.Card.CardNumber,
                ExpiryMonth = request.Card.Expiry.Month,
                ExpiryYear = request.Card.Expiry.Year
            };
            ScreeningResponse screeningResponseFromFraudApi = new() { Accepted = true };

            _fraudApi
                .Setup(fraud => fraud.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()))
                .Callback<ScreeningRequest, CancellationToken>((screeningRequest, ct) => screeningRequestSentToFraudApi = screeningRequest)
                .ReturnsAsync(screeningResponseFromFraudApi);

            AuthorisationRequest? authorisationRequestSentToBankApi = null;
            AuthorisationRequest expectedAuthorisationRequest = new();
            AuthorisationResponse authorisationResponseFromBankApi = new();

            _bankApi
                .Setup(bank => bank.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()))
                .Callback<AuthorisationRequest, CancellationToken>((authorisationRequest, ct) => authorisationRequestSentToBankApi = authorisationRequest)
                .ReturnsAsync(authorisationResponseFromBankApi);

            var verify = Verify(await _handler.PostPaymentAsync(request, default)).ScrubInlineGuids();

            _fraudApi.Verify(api => api.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            screeningRequestSentToFraudApi.ShouldBeEquivalentTo(expectedScreeningRequest);

            _bankApi.Verify(api => api.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            authorisationRequestSentToBankApi.ShouldBeEquivalentTo(expectedAuthorisationRequest);

            var paymentFateInstrument = _paymentFateCountCollector.GetMeasurementSnapshot();
            paymentFateInstrument.Count.ShouldBe(1);

            var paymentRequestedInstrument = _paymentRequestedCountCollector.GetMeasurementSnapshot();
            paymentRequestedInstrument.Count.ShouldBe(1);

            await verify;
        }
    }
}
