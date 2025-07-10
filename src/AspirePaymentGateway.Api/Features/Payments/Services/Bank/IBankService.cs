using AspirePaymentGateway.Api.Features.Payments.Domain;
using BinaryMash.Extensions.Results;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Bank;

public interface IBankService
{
    Task<Result<AuthorisationDetails>> AuthorisePaymentAsync(Payment payment, CancellationToken cancellationToken);
}
