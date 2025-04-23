using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace BinaryMash.Extensions.Keycloak
{
    internal class RoleClaimsTransformation : IClaimsTransformation
    {
        readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;

            // roles are serialized under the realm_access claim
            var realmAccessClaim = identity?.FindFirst("realm_access");
            if (realmAccessClaim != null)
            {
                // Deserialize the realm_access JSON to extract the roles
                var realmAccess = JsonSerializer.Deserialize<RealmAccess>(realmAccessClaim.Value, _options);

                if (realmAccess?.Roles != null)
                {
                    foreach (var role in realmAccess.Roles)
                    {
                        // Add each role as a Claim of type ClaimTypes.Role
                        identity?.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            return Task.FromResult(principal);
        }

        public class RealmAccess
        {
            public List<string>? Roles { get; set; }
        }
    }
}
