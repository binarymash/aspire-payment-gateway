namespace AspirePaymentGateway.Api.Features.Payments.Domain
{
    public class Amount
    {
        public required long ValueInMinorUnits { get; init; }

        public required string CurrencyCode { get; init; }
    }
}
