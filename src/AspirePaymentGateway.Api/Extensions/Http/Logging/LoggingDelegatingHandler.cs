namespace AspirePaymentGateway.Api.Extensions.Http.Logging
{
    public partial class LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LogBeforeSendRequest();

            var response = await base.SendAsync(request, cancellationToken);

            LogAfterReceiveResponse();

            return response;
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Phil is cool")]
        partial void LogBeforeSendRequest();

        [LoggerMessage(Level = LogLevel.Information, Message = "Phil is still cool")]
        partial void LogAfterReceiveResponse();

    }
}
