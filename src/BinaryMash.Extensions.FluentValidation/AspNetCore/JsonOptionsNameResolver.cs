using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace BinaryMash.Extensions.FluentValidation.AspNetCore
{
    public class JsonOptionsNameResolver
    {
        private readonly JsonOptions _jsonOptions;

        public JsonOptionsNameResolver(IOptions<JsonOptions> jsonOptions)
        {
            _jsonOptions = jsonOptions.Value;
        }

        public string ResolveName(Type type, MemberInfo memberInfo, LambdaExpression expression)
        {
            return _jsonOptions.SerializerOptions.PropertyNamingPolicy?.ConvertName(memberInfo?.Name ?? string.Empty) ?? string.Empty;
        }
    }
}
