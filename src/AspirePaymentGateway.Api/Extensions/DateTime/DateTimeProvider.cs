namespace AspirePaymentGateway.Api.Extensions.DateTime
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public System.DateTime UtcNow => System.DateTime.UtcNow;
    }
}
