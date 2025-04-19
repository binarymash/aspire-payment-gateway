using Shouldly;

namespace AspirePaymentGateway.Tests.Payments.Failures;

[Collection(nameof(PaymentCollection))]
public class Http404NotFoundTests(PaymentFixture fixture)
{
    [Fact]
    public async Task GetUnknownPaymentReturns404NotFound()
    {
        //Arrange
        var paymentId = $"pay_{Guid.NewGuid()}";
        var paymentUri = new Uri($"/payments/{paymentId}", UriKind.Relative);

        using var getPaymentRequest = fixture.PaymentGateway.GetPaymentRequest
            .WithLocation(paymentUri);

        // Act
        var response = await getPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
