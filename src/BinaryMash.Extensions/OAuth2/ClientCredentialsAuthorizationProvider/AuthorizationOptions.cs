namespace BinaryMash.Extensions.OAuth2.ClientCredentialsAuthorizationProvider
{
    public record AuthorizationOptions(string Realm, string ClientId, string ClientSecret, int RenewalPeriodBeforeExpiryInSeconds = 60);
}
