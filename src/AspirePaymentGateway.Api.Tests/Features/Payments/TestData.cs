using static AspirePaymentGateway.Api.Features.Payments.Contracts;

namespace AspirePaymentGateway.Api.Tests.Features.Payments
{
    internal static class TestData
    {
        internal static class PaymentRequests
        {
            internal static PaymentRequest Nominal => new(
                new CardDetails(
                    CardNumber: "4444333322221111",
                    CardHolderName: "Philip Wood",
                    Expiry: new CardExpiry(
                        Month: 5,
                        Year: 2029),
                    CVV: 123),
                new PaymentDetails(
                    Amount: 4567L,
                    CurrencyCode: "GBP"));
        }

        internal static class Payment
        {
            internal static Api.Features.Payments.Domain.Payment AcceptedByBank(string paymentId)
            {
                var payment = Api.Features.Payments.Domain.Payment.Create(
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

            internal static Api.Features.Payments.Domain.Payment DeclinedByBank(string paymentId)
            {
                var payment = Api.Features.Payments.Domain.Payment.Create(
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

                return payment;
            }
        }
    }
}
