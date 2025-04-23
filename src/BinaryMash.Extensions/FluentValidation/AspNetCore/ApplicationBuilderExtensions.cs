using BinaryMash.Extensions.FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Hosting
#pragma warning restore IDE0130 // Namespace does not match folder structure
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
