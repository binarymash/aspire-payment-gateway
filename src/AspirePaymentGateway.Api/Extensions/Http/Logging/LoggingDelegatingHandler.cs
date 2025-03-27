namespace AspirePaymentGateway.Api.Extensions.Http.Logging
{
    public partial class LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LogBeforeSendRequest(await request.Content?.ReadAsStringAsync());

            var response = await base.SendAsync(request, cancellationToken);

            LogAfterReceiveResponse(await response.Content?.ReadAsStringAsync());

            return response;
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Request: {RequestBody}")]
        partial void LogBeforeSendRequest(string? requestBody);

        [LoggerMessage(Level = LogLevel.Information, Message = "Response: {ResponseBody}")]
        partial void LogAfterReceiveResponse(string? responseBody);

    }
}
