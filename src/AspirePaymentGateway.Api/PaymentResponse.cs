namespace AspirePaymentGateway.Api
{
    public record PaymentResponse
    {
        public required string PaymentId { get; init; }

        public required string Status { get; init; }
    }
}
