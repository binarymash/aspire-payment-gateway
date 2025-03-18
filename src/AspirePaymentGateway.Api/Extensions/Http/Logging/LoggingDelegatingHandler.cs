namespace AspirePaymentGateway.Api.Extensions.Http.Logging
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;

        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Phil is cool");
            
            var response = await base.SendAsync(request, cancellationToken);
            
            _logger.LogInformation("Phil is still cool");
            
            return response;
        }
    }
}
