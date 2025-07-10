namespace AspirePaymentGateway.Api.Features.Payments.Domain;

public record AuthorisationDetails
{
    public required bool Authorised { get; set; }

    public required string AuthorisationRequestId { get; set; }

    public string? AuthorisationCode { get; set; }
}
