using AspirePaymentGateway.Api.Features.Payments.Domain;
using AspirePaymentGateway.Api.Features.Payments.Services.Bank.HttpApi;
using BinaryMash.Extensions.Results;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Bank;

public class BankService(IBankApi api) : IBankService
{
    async Task<Result<AuthorisationDetails>> IBankService.AuthorisePaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        var authorisationRequest = new HttpApi.Contracts.AuthorisationRequest
        {
            AuthorisationRequestId = Guid.NewGuid().ToString(),
            Pan = payment.Card.CardNumber,
            CardHolderFullName = payment.Card.CardHolderName,
            Cvv = payment.Card.CVV,
            ExpiryMonth = payment.Card.Expiry.Month,
            ExpiryYear = payment.Card.Expiry.Year,
            Amount = payment.Amount.ValueInMinorUnits,
            CurrencyCode = payment.Amount.CurrencyCode
        };

        try
        {
            var response = await api.AuthoriseAsync(authorisationRequest, cancellationToken);

            return new AuthorisationDetails
            {
                Authorised = response.Authorised,
                AuthorisationRequestId = response.AuthorisationRequestId,
                AuthorisationCode = response.AuthorisationCode,
            };
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthorisationDetails>(new Errors.BankApiExceptionError(ex));
        }
    }
}
