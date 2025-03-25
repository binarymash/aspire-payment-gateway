namespace AspirePaymentGateway.Api.Extensions.Currencies
{
    public static class IsoCurrencyCodes
    {
        public static readonly string GBP = "GBP";

        public static readonly string USD = "USD";

        public static readonly string EUR = "EUR";

        public static readonly HashSet<string> All = new([GBP, USD, EUR]);
    }
}
