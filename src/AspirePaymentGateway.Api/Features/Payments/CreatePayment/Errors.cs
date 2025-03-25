using FluentResults;

namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment
{
    public static partial class Errors
    {
        public static ScreeningFailure ScreeningFailure() => new ScreeningFailure();
    }

    public class ScreeningFailure : Error
    {
    }
}
