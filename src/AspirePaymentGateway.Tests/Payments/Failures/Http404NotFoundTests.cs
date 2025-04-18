using Shouldly;

namespace AspirePaymentGateway.Tests.Payments.Failures;

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

        using var getPaymentRequest = _fixture.PaymentGateway.GetPaymentRequest
            .WithLocation(paymentUri);

        // Act
        var response = await getPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
