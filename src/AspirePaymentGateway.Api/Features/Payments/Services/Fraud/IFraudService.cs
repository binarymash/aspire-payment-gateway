using AspirePaymentGateway.Api.Features.Payments.Domain;
using BinaryMash.Extensions.Results;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Fraud;

public interface IFraudService
{
    Task<Result<bool>> ScreenPaymentAsync(Payment payment, CancellationToken cancellationToken);
}
