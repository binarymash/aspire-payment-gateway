namespace AspirePaymentGateway.Api.Middleware
{
    /// <summary>
    /// Global exception handler middleware
    /// </summary>
    /// <remarks>
    /// We should aim to catch and handle all exceptions in the individual endpoint handlers, as this lets
    /// us return the most appropriate domain-aware status code and error details to the client.
    /// As such, we should never hit this middleware. However, we have it as a final failsafe so as to
    /// not expose exception details over the API
    /// </remarks>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    public partial class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                LogException(ex);

                IResult result = Results.Problem(title: "An unexpected error occurred");

                await result.ExecuteAsync(context);
            }
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "An unhandled exception occurred")]
        partial void LogException(Exception exception);
    }
}
