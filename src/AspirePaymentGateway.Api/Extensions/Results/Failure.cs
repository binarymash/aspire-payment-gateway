namespace AspirePaymentGateway.Api.Extensions.Results
{
    public record Failure
    {
        public required string Message { get; set; }

        public required string Code { get; set; }

        public bool IsTransient { get; set; }
    }
}
