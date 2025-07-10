using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Bank.HttpApi;

public static class Contracts
{
    public record AuthorisationRequest
    {
        public required string AuthorisationRequestId { get; set; }
        public required string Pan { get; set; }
        public required string CardHolderFullName { get; set; }
        public required int Cvv { get; set; }
        public required int ExpiryMonth { get; set; }
        public required int ExpiryYear { get; set; }
        public required long Amount { get; set; }
        public required string CurrencyCode { get; set; }
    }

#pragma warning disable CS8618 // Marking properties as required will break deserialization
    public record AuthorisationResponse
    {
        public string AuthorisationRequestId { get; set; }

        public bool Authorised { get; set; }

        public string? AuthorisationCode { get; set; }
    }
#pragma warning restore CS8618 
}

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(Contracts.AuthorisationRequest))]
[JsonSerializable(typeof(Contracts.AuthorisationResponse))]
internal partial class BankApiContractsContext : JsonSerializerContext;
