namespace AspirePaymentGateway.Api.Extensions.Http.Logging
{
    public class AnotherLoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;

        public AnotherLoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Phil. Cool is he?");
            
            var response = await base.SendAsync(request, cancellationToken);
            
            _logger.LogInformation("Yes. He is cool");
            
            return response;
        }
    }
}
