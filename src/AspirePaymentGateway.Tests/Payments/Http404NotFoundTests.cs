using Shouldly;
using System.Net.Http.Headers;

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

        var token = await _fixture.IdentityServer.GetPaymentGatewayTokenAsync(TestContext.Current.CancellationToken);

        using var message = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/payments/{paymentId}", UriKind.Relative),
        };

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _fixture.PaymentGateway.SendAsync(message, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
