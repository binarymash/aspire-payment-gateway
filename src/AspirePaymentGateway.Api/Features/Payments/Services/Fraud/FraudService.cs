using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Services.Fraud.HttpApi;
using BinaryMash.Extensions.Results;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Fraud;

public class FraudService(IFraudApi api) : IFraudService
{
    public async Task<Result<bool>> ScreenPaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            var screeningRequest = new HttpApi.Contracts.ScreeningRequest
            {
                CardNumber = payment.Card.CardNumber,
                CardHolderName = payment.Card.CardHolderName,
                ExpiryMonth = payment.Card.Expiry.Month,
                ExpiryYear = payment.Card.Expiry.Year
            };

            var screeningResponse = await api.DoScreeningAsync(screeningRequest, cancellationToken);

            return screeningResponse.Accepted;
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(new Errors.FraudApiExceptionError(ex));
        }
    }
}
