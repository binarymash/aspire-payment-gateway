using System.Net;

namespace AspirePaymentGateway.Api.Extensions.Http.Logging
{
    public partial class AnotherLoggingDelegatingHandler(ILogger<AnotherLoggingDelegatingHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LogBeforeSendRequest(new MyRecord(123, "ABC"), request.Method, request.RequestUri);

            var response = await base.SendAsync(request, cancellationToken);

            LogAfterReceiveResponse(response.StatusCode);

            return response;
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Phil. Cool is he? {Method} {Path} {TheRecord}")]
        partial void LogBeforeSendRequest(MyRecord theRecord, HttpMethod method, System.Uri? path);

        [LoggerMessage(Level = LogLevel.Information, Message = "Yes. He is cool {StatusCode}")]
        partial void LogAfterReceiveResponse(HttpStatusCode statusCode);

        private record MyRecord(int Something, string Nothing);
    }
}
