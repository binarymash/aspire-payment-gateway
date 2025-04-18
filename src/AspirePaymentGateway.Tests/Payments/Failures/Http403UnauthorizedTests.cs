using Shouldly;

namespace AspirePaymentGateway.Tests.Payments.Failures;

[Collection(nameof(PaymentCollection))]
public class Http403UnauthorizedTests
{
    private readonly PaymentFixture _fixture;

    public Http403UnauthorizedTests(PaymentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateWithoutBearerTokenReturns403Unauthorised()
    {
        // Create payment

        //Arrange
        using var createPaymentRequest = _fixture.PaymentGateway.CreatePaymentRequest
            .WithContent(TestData.PaymentRequest.Nominal)
            .WithoutBearerToken();

        // Act
        var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

        // Assert
        createPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        createPaymentResponse.Headers.Location.ShouldBeNull();
    }

    [Fact]
    public async Task CreateWithExpiredTokenReturns403Unauthorised()
    {
        // Create payment

        //Arrange
        using var createPaymentRequest = _fixture.PaymentGateway.CreatePaymentRequest
            .WithContent(TestData.PaymentRequest.Nominal)
            .WithBearerToken(TestData.BearerToken.PaymentGatewayCustomer.Expired);

        // Act
        var createPaymentResponse = await createPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

        // Assert
        createPaymentResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        createPaymentResponse.Headers.Location.ShouldBeNull();
    }

    [Fact]
    public async Task GetWithoutBearerTokenReturns403Unauthorised()
    {
        // Arrange
        var paymentId = $"pay_{Guid.NewGuid()}";
        var paymentUri = new Uri($"/payments/{paymentId}", UriKind.Relative);
        
        using var getPaymentRequest = _fixture.PaymentGateway.GetPaymentRequest
            .WithLocation(paymentUri)
            .WithoutBearerToken();

        // Act
        var response = await getPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWithExpiredBearerTokenReturns403Unauthorised()
    {
        // Arrange
        var paymentId = $"pay_{Guid.NewGuid()}";
        var paymentUri = new Uri($"/payments/{paymentId}", UriKind.Relative);

        using var getPaymentRequest = _fixture.PaymentGateway.GetPaymentRequest
            .WithLocation(paymentUri)
            .WithBearerToken(TestData.BearerToken.PaymentGatewayCustomer.Expired);

        // Act
        var response = await getPaymentRequest.SendAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
