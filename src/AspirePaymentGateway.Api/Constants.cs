namespace AspirePaymentGateway.Api
{
    public static class Constants
    {
        /// <summary>
        /// Ensure that these match what is set up in <see cref="AspirePaymentGateway.AppHost.Program"/>
        /// </summary>
        public static class BaseUrls
        {
            public const string FraudApi = "https://fraud-api";

            public const string BankApi = "https://mock-bank-api";

            public const string IdentityServer = "https://keycloak";
        }
    }
}
