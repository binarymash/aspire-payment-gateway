using BinaryMash.Extensions.FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFluentValidationNamingFromJsonOptions(this IApplicationBuilder app)
        {
            var resolver = new JsonOptionsNameResolver(app.ApplicationServices.GetRequiredService<IOptions<JsonOptions>>());

            ValidatorOptions.Global.PropertyNameResolver = resolver.ResolveName;
            ValidatorOptions.Global.DisplayNameResolver = resolver.ResolveName;

            return app;
        }
    }
}
