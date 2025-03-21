namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.FraudApi
{
    public record ScreeningResponse
    {
        public bool Accepted { get; init; }
    }
}