using System.Text.Json;

namespace AspirePaymentGateway.Api.Storage.CosmosDb
{
    public static class CosmosSerializerOptions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static JsonSerializerOptions Options => _options;
    }
}
