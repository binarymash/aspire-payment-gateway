using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Features.Payments.CreatePayment.BankApi
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
