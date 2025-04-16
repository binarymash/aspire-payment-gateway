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

            //arrange request from client
            PaymentRequest paymentRequest = TestData.PaymentRequests.Nominal;

            // arrange fraud api response
            ScreeningRequest? screeningRequestSentToFraudApi = null;
            ScreeningResponse screeningResponseFromFraudApi = new() { Accepted = true };

            Fixture.FraudApi
                .Setup(fraud => fraud.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()))
                .Callback<ScreeningRequest, CancellationToken>((screeningRequest, ct) => screeningRequestSentToFraudApi = screeningRequest)
                .ReturnsAsync(screeningResponseFromFraudApi);

            // arrange bank api response
            AuthorisationRequest? authorisationRequestSentToBankApi = null;
            AuthorisationResponse authorisationResponseFromBankApi = null!;

            Fixture.BankApi
                .Setup(bank => bank.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AuthorisationRequest authorisationRequest, CancellationToken ct) =>
                {
                    authorisationRequestSentToBankApi = authorisationRequest;
                    
                    authorisationResponseFromBankApi = new AuthorisationResponse
                    {
                        AuthorisationRequestId = authorisationRequest.AuthorisationRequestId,
                        Authorised = true,
                        AuthorisationCode = "ABCDE"
                    };

                    return authorisationResponseFromBankApi;
                });

            // act
            var result = await Fixture.CreatePaymentHandler.PostPaymentAsync(paymentRequest, TestContext.Current.CancellationToken);
            
            // assert

            // assert response to client
            await Verify(result).ScrubInlineGuids();

            // assert fraud api response
            Fixture.FraudApi.Verify(api => api.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            screeningRequestSentToFraudApi.ShouldBeEquivalentTo(new ScreeningRequest
            {
                CardHolderName = paymentRequest.Card.CardHolderName,
                CardNumber = paymentRequest.Card.CardNumber,
                ExpiryMonth = paymentRequest.Card.Expiry.Month,
                ExpiryYear = paymentRequest.Card.Expiry.Year
            });

            // assert bank api response
            Fixture.BankApi.Verify(api => api.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            authorisationRequestSentToBankApi.ShouldBeEquivalentTo(new AuthorisationRequest
            { 
                AuthorisationRequestId = authorisationResponseFromBankApi.AuthorisationRequestId 
            });

            // assert metrics
            var paymentFateInstrument = Fixture.PaymentFateCountCollector.GetMeasurementSnapshot();
            paymentFateInstrument.Count.ShouldBe(1);

            var paymentRequestedInstrument = Fixture.PaymentRequestedCountCollector.GetMeasurementSnapshot();
            paymentRequestedInstrument.Count.ShouldBe(1);
        }

        [Fact]
        public async Task NominalRequestWhichIsDeclinedByBank()
        {
            // arrange

            // arrange request from client
            PaymentRequest paymentRequest = TestData.PaymentRequests.Nominal;

            // arrange fraud api response
            ScreeningRequest? screeningRequestSentToFraudApi = null;
            ScreeningResponse screeningResponseFromFraudApi = new() { Accepted = true };

            Fixture.FraudApi
                .Setup(fraud => fraud.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()))
                .Callback<ScreeningRequest, CancellationToken>((screeningRequest, ct) => screeningRequestSentToFraudApi = screeningRequest)
                .ReturnsAsync(screeningResponseFromFraudApi);

            // arrange bank api response
            AuthorisationRequest? authorisationRequestSentToBankApi = null;
            AuthorisationResponse authorisationResponseFromBankApi = null!;

            Fixture.BankApi
                .Setup(bank => bank.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AuthorisationRequest authorisationRequest, CancellationToken ct) =>
                {
                    authorisationRequestSentToBankApi = authorisationRequest;

                    authorisationResponseFromBankApi = new()
                    {
                        AuthorisationRequestId = authorisationRequest.AuthorisationRequestId,
                        Authorised = false,
                        AuthorisationCode = "GHIJK",
                    };

                    return authorisationResponseFromBankApi;
                });

            // act
            var result = await Fixture.CreatePaymentHandler.PostPaymentAsync(paymentRequest, TestContext.Current.CancellationToken);

            // assert 

            // assert response to client
            await Verify(result).ScrubInlineGuids();

            // assert fraud api request
            Fixture.FraudApi.Verify(api => api.DoScreening(It.IsAny<ScreeningRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            screeningRequestSentToFraudApi.ShouldBeEquivalentTo(new ScreeningRequest
            {
                CardHolderName = paymentRequest.Card.CardHolderName,
                CardNumber = paymentRequest.Card.CardNumber,
                ExpiryMonth = paymentRequest.Card.Expiry.Month,
                ExpiryYear = paymentRequest.Card.Expiry.Year
            });

            // assert bank api request
            Fixture.BankApi.Verify(api => api.AuthoriseAsync(It.IsAny<AuthorisationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            authorisationRequestSentToBankApi.ShouldBeEquivalentTo(new AuthorisationRequest
            {
                AuthorisationRequestId = authorisationResponseFromBankApi.AuthorisationRequestId
            });

            // assert metrics
            var paymentFateInstrument = Fixture.PaymentFateCountCollector.GetMeasurementSnapshot();
            paymentFateInstrument.Count.ShouldBe(1);

            var paymentRequestedInstrument = Fixture.PaymentRequestedCountCollector.GetMeasurementSnapshot();
            paymentRequestedInstrument.Count.ShouldBe(1);
        }
    }
}
