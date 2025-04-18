using Shouldly;

namespace AspirePaymentGateway.Tests.Payments;

[Collection(nameof(PaymentCollection))]
public class Http403UnauthorizedTests
{
    private readonly PaymentFixture _fixture;

    public Http403UnauthorizedTests(PaymentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetWithoutTokenReturns403Unauthorised()
    {
        // Act
        var response = await _fixture.PaymentGateway.GetAsync("/payments/abcdefg", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldBeEmpty();
    }
}
