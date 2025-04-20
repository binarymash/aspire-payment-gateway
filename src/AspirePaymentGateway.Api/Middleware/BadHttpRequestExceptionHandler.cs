using AspirePaymentGateway.Api.Telemetry;

namespace AspirePaymentGateway.Api.Middleware
{
    /// <summary>
    /// Middleware to handle BadHttpRequestException.
    /// </summary>
    /// <remarks>
    /// Normal request validation should occur within the endpoint handlers; this middleware is
    /// for catching exceptions that prevent the request from reaching the handlers - ie, null or
    /// malformed requests that cannot be deserialized.
    /// </remarks>
    /// <param name="next"></param>
    /// <param name="metrics"></param>
    public class BadHttpRequestExceptionHandler(RequestDelegate next, BusinessMetrics metrics)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BadHttpRequestException)
            {
                metrics.RecordPaymentRequestRejected();

                IResult result = Results.ValidationProblem(errors: [], title: "The request was malformed");

                await result.ExecuteAsync(context);
            }
        }
    }
}
