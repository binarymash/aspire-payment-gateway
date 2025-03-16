using AspirePaymentGateway.Api.Extensions.FluentValidation;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting
{
    public static class ApplicationBuilderExteneions
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
