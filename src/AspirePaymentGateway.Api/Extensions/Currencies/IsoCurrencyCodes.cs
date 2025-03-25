namespace AspirePaymentGateway.Api.Extensions.Currencies
{
    public static class IsoCurrencyCodes
    {
        public static string GBP = "GBP";

        public static string USD = "USD";

        public static string EUR = "EUR";

        public static HashSet<string> All = new HashSet<string>([GBP, USD, EUR]);
    }
}
