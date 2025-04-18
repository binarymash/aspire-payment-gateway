namespace AspirePaymentGateway.Tests.Payments
{
    public static class TestData
    {
        public static class PaymentRequest
        {
            public static Dictionary<string, object> Nominal =>
                new Dictionary<string, object>
                {
                { "card", new Dictionary<string, object>
                    {
                        { "card_number", "4444333322221111" },
                        { "card_holder_name", "Philip Wood" },
                        { "expiry", new Dictionary<string, object>
                            {
                                { "month", 7 },
                                { "year", 2025 }
                            }
                        },
                        { "cvv", 123 }
                    }
                },
                { "payment", new Dictionary<string, object>
                    {
                        { "amount", 999 },
                        { "currency_code", "GBP" }
                    }
                }
            };

            public static Dictionary<string, object> Invalid =>
                new Dictionary<string, object>
                {
                { "card", new Dictionary<string, object>
                    {
                        { "card_number", "444433332222111" },
                        { "card_holder_name", "Philip Wood" },
                        { "expiry", new Dictionary<string, object>
                            {
                                { "month", 17 },
                                { "year", 2023 }
                            }
                        },
                        { "cvv", 123456 }
                    }
                },
                { "payment", new Dictionary<string, object>
                    {
                        { "amount", -1 },
                        { "currency_code", "GBP" }
                    }
                }
            };

            public static Dictionary<string, object> Empty => new Dictionary<string, object>();

            public static Dictionary<string, object> ThrowsExceptionInFraudApi =>
                new Dictionary<string, object>
                {
                { "card", new Dictionary<string, object>
                    {
                        { "card_number", "4444333322221177" },
                        { "card_holder_name", "Philip Wood" },
                        { "expiry", new Dictionary<string, object>
                            {
                                { "month", 7 },
                                { "year", 2025 }
                            }
                        },
                        { "cvv", 123 }
                    }
                },
                { "payment", new Dictionary<string, object>
                    {
                        { "amount", 999 },
                        { "currency_code", "GBP" }
                    }
                }
            };
        }

        public static class BearerToken
        {
            public static class PaymentGatewayCustomer
            {
                public static string Expired =>
                    "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJYY3BlWUtTYnRTUVpSQ19LTkQxRUItZGJlV19wSWdzWlJDRjUyeWNjSWRnIn0.eyJleHAiOjE3NDQ5OTYyMzYsImlhdCI6MTc0NDk5NTkzNiwianRpIjoiYjMzOTVmODItNTcwYi00N2JlLTk4NzgtZjFjYzJhZmZiOThlIiwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo4MDgwL3JlYWxtcy9wYXltZW50LWdhdGV3YXkiLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiN2M1MzgxYTctNmYwMC00MjY2LTk5NGYtN2MzOTkyODA3ZmFlIiwidHlwIjoiQmVhcmVyIiwiYXpwIjoicGF5bWVudC1nYXRld2F5LWN1c3RvbWVyIiwic2lkIjoiODcwYmFjYmMtMDYwYy00OGM2LTg1YjgtNTJlNTJmNjU5ZjZjIiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyIvKiJdLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsib2ZmbGluZV9hY2Nlc3MiLCJkZWZhdWx0LXJvbGVzLXBheW1lbnQtZ2F0ZXdheSIsInVtYV9hdXRob3JpemF0aW9uIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJvcGVuaWQgZW1haWwgcHJvZmlsZSIsImVtYWlsX3ZlcmlmaWVkIjp0cnVlLCJuYW1lIjoiVGVzdCBVc2VyIiwicHJlZmVycmVkX3VzZXJuYW1lIjoidGVzdEB0ZXN0LmNvbSIsImdpdmVuX25hbWUiOiJUZXN0IiwiZmFtaWx5X25hbWUiOiJVc2VyIiwiZW1haWwiOiJ0ZXN0QHRlc3QuY29tIn0.glnsFDw0fWgVATWKBKt2IFdpew4i2FuNwVFv2qYJBtMCgltTnsUJGYiKedW6mjAznbEA5FaTUkx5HkHfAp7BJd429vK7Nl9HZNuOACXlRvSs9JypqXxvxlbChFNO5nk7BlCP0C6AM4LDNWby8g6eWf3BU3GOzPs8G7aQU9W2OJtFQOquDfe_OW-gLIELHguPec8pJSE5bWTOu1GdzzRvVt4sBzk9--D6FGFSPytBJkxZHy41QPcLFPAD37MNXttg_Hq0s8e9v5ihRzGjhnwniM1GQYTQh56qtfIy46iWTXBa4DcabSptBcx4gkcLTlhVpJ5eR6KDfkhqFrObmV2p0g";
            }
        }
    }
}
