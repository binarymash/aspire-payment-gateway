using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Features.Payments.Services.BankApi
{
    public static class Contracts
    {
        public record AuthorisationRequest
        {
            public required string AuthorisationRequestId { get; init; }
        }

        public record AuthorisationResponse
        {
            public required string AuthorisationRequestId { get; init; }

            public required bool Authorised { get; init; }

            public string? AuthorisationCode { get; init; }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(Contracts.AuthorisationRequest))]
    [JsonSerializable(typeof(Contracts.AuthorisationResponse))]
    internal partial class BankApiContractsContext : JsonSerializerContext;
}
