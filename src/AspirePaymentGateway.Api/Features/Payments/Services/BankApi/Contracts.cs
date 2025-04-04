using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Features.Payments.Services.BankApi
{
    public static class Contracts
    {
        public record AuthorisationRequest
        {
        }

        public record AuthorisationResponse
        {
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(Contracts.AuthorisationRequest))]
    [JsonSerializable(typeof(Contracts.AuthorisationResponse))]
    internal partial class BankApiContractsContext : JsonSerializerContext;
}
