
namespace AspirePaymentGateway.Api.Extensions.DateTime
{
    public class StubbedDateTimeProvider(System.DateTime utcNow) : IDateTimeProvider
    {
        public System.DateTime UtcNow => utcNow;
    }
}
