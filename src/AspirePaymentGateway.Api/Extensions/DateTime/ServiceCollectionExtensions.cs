using AspirePaymentGateway.Api.Extensions.DateTime;

namespace Microsoft.Extensions.Hosting
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
