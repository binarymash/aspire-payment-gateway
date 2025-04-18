using Shouldly;

namespace AspirePaymentGateway.Tests.Payments;

[Collection(nameof(PaymentCollection))]
public class Http404NotFoundTests
{
    private readonly PaymentFixture _fixture;

    public Http404NotFoundTests(PaymentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetUnknownPaymentReturns404NotFound()
    {
        //Arrange
        var paymentId = $"pay_{Guid.NewGuid()}";
        var paymentUri = new Uri($"/payments/{paymentId}", UriKind.Relative);

        // Act
        var response = await _fixture.PaymentGateway.GetPaymentAsync(paymentUri, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
