namespace AspirePaymentGateway.Api.Extensions.DateTime
{
    public interface IDateTimeProvider
    {
        System.DateTime UtcNow { get; }

        string UtcNowAsString => UtcNow.ToString("O");
    }
}
