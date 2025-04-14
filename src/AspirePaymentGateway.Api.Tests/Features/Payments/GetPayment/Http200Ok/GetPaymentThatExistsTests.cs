using AspirePaymentGateway.Api.Features.Payments.Domain;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.GetPayment.Http200Ok
{
    public class GetPaymentThatExistsTests : ComponentTests
    {
        public GetPaymentThatExistsTests()
        {

        }

        [Fact]
        public async Task GetAcceptedPayment()
        {
            // Arrange
            string paymentId = $"pay_{Guid.NewGuid().ToString()}";

            var payment = Payment.Create(
                paymentId,
                amountInMinorUnits: 12453,
                currencyIsoCode: "GBP",
                cardNumber: "41234567812345678",
                cardHolderName: "John Doe",
                cvv: 655,
                expiryMonth: 7,
                expiryYear: 2026);
            payment.RecordScreeningResponse(true);
            payment.RecordAuthorisationResponse(
                authorisationRequestId: "abcdef",
                isAuthorised: true,
                authorisationCode: "184698");

            await Repository.SaveAsync(payment.UncommittedEvents, default);

            // Act
            var response = await GetPaymentHandler.GetPaymentAsync(paymentId, default);

            // Assert
            await Verify(response).ScrubInlineGuids();
        }

        [Fact]
        public async Task GetPaymentDeclinedByBank()
        {
            // Arrange
            string paymentId = $"pay_{Guid.NewGuid().ToString()}";

            var payment = Payment.Create(
                paymentId,
                amountInMinorUnits: 12453,
                currencyIsoCode: "GBP",
                cardNumber: "41234567812345678",
                cardHolderName: "John Doe",
                cvv: 655,
                expiryMonth: 7,
                expiryYear: 2026);
            payment.RecordScreeningResponse(true);
            payment.RecordAuthorisationResponse(
                authorisationRequestId: "abcdef",
                isAuthorised: false,
                authorisationCode: "184698");

            await Repository.SaveAsync(payment.UncommittedEvents, default);

            // Act
            var response = await GetPaymentHandler.GetPaymentAsync(paymentId, default);

            // Assert
            await Verify(response).ScrubInlineGuids();
        }

        Payment PaymentAcceptedByBank(string paymentId)
        {
            var payment = Payment.Create(
                paymentId,
                amountInMinorUnits: 12453,
                currencyIsoCode: "GBP",
                cardNumber: "41234567812345678",
                cardHolderName: "John Doe",
                cvv: 655,
                expiryMonth: 7,
                expiryYear: 2026);
            payment.RecordScreeningResponse(true);
            payment.RecordAuthorisationResponse(
                authorisationRequestId: "abcdef",
                isAuthorised: true,
                authorisationCode: "184698");

            return payment;
        }

        Payment PaymentDeclinedByBank(string paymentId)
        {
            var payment = Payment.Create(
                paymentId,
                amountInMinorUnits: 12453,
                currencyIsoCode: "GBP",
                cardNumber: "41234567812345678",
                cardHolderName: "John Doe",
                cvv: 655,
                expiryMonth: 7,
                expiryYear: 2026);
            payment.RecordScreeningResponse(true);
            payment.RecordAuthorisationResponse(
                authorisationRequestId: "abcdef",
                isAuthorised: true,
                authorisationCode: "184698");

            return payment;
        }
    }
}
