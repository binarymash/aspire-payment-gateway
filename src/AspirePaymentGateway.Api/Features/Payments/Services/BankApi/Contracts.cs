using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Features.Payments.Services.BankApi
{
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

        public record AuthorisationResponse
        {
            public required string AuthorisationRequestId { get; set; }

            public required bool Authorised { get; set; }

            public string? AuthorisationCode { get; set; }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(Contracts.AuthorisationRequest))]
    [JsonSerializable(typeof(Contracts.AuthorisationResponse))]
    internal partial class BankApiContractsContext : JsonSerializerContext;
}
