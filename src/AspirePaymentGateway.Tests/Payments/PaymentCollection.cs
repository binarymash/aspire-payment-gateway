namespace AspirePaymentGateway.Tests.Payments
{
    [CollectionDefinition(nameof(PaymentCollection))]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class PaymentCollection : ICollectionFixture<PaymentFixture>;
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
}
