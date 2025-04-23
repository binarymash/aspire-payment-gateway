using BinaryMash.Extensions.DateTime;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Hosting
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStandardDateTimeProvider(this IServiceCollection services)
        {
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            return services;
        }
    }
}
