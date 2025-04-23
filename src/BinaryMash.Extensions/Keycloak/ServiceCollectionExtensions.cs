#pragma warning disable IDE0130 // Namespace does not match folder structure
using BinaryMash.Extensions.Keycloak;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Keycloak Realm-level roles need to be unwrapped from the realm_access claim
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddKeycloakRoleClaimsTransformation(this IServiceCollection services)
        {
            return services.AddSingleton<IClaimsTransformation, RoleClaimsTransformation>();
        }
    }
}
