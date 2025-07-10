using System.Text.Json.Serialization;

namespace AspirePaymentGateway.Api.Features.Payments.Services.Fraud.HttpApi;

public static class Contracts
{
    public record ScreeningRequest
    {
        public required string CardNumber { get; set; }

        public required string CardHolderName { get; set; }

        public required int ExpiryMonth { get; set; }

        public required int ExpiryYear { get; set; }
    }

    public record ScreeningResponse
    {
        public required bool Accepted { get; set; }
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Contracts.ScreeningRequest))]
[JsonSerializable(typeof(Contracts.ScreeningResponse))]
internal partial class FraudApiContractsContext : JsonSerializerContext;
