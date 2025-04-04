namespace AspirePaymentGateway.Api.Features.Payments.Domain
{
    public static class PaymentStatus
    {
        public const string None = "NONE";
        public const string Pending = "PENDING";
        public const string Screened = "SCREENED";
        public const string Authorised = "AUTHORISED";
        public const string Declined = "DECLINED";
    }
}
