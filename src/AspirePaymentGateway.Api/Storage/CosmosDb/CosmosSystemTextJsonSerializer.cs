using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;

namespace AspirePaymentGateway.Api.Storage.CosmosDb
{
    public class CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions) : CosmosLinqSerializer
    {
        public JsonSerializerOptions JsonSerializerOptions => jsonSerializerOptions;

        private readonly JsonObjectSerializer systemTextJsonSerializer = new JsonObjectSerializer(jsonSerializerOptions);

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (stream.CanSeek
                       && stream.Length == 0)
                {
                    return default;
                }

                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                return (T)this.systemTextJsonSerializer.Deserialize(stream, typeof(T), default);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            MemoryStream streamPayload = new MemoryStream();
            this.systemTextJsonSerializer.Serialize(streamPayload, input, input.GetType(), default);
            streamPayload.Position = 0;
            return streamPayload;
        }

        public override string SerializeMemberName(MemberInfo memberInfo)
        {
            JsonExtensionDataAttribute jsonExtensionDataAttribute = memberInfo.GetCustomAttribute<JsonExtensionDataAttribute>(true);
            if (jsonExtensionDataAttribute != null)
            {
                return null;
            }

            JsonPropertyNameAttribute jsonPropertyNameAttribute = memberInfo.GetCustomAttribute<JsonPropertyNameAttribute>(true);
            if (!string.IsNullOrEmpty(jsonPropertyNameAttribute?.Name))
            {
                return jsonPropertyNameAttribute.Name;
            }

            if (jsonSerializerOptions.PropertyNamingPolicy != null)
            {
                return jsonSerializerOptions.PropertyNamingPolicy.ConvertName(memberInfo.Name);
            }

            // Do any additional handling of JsonSerializerOptions here.

            return memberInfo.Name;
        }
    }
    // </SystemTextJsonSerializer>
}
