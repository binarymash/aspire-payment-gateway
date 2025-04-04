using AspirePaymentGateway.Api.Extensions.Results;

namespace AspirePaymentGateway.Api.Features.Payments.Domain
{
    public static class Errors
    {
        public record ValidationError() : ErrorDetail("SOP_001", "There was a validation error");

        public record PaymentNotFoundError() : ErrorDetail("SOP_123", "The payment was not found");

        public record UnknownEventTypeError(string EventType) : ErrorDetail("SOP_456", "An unknown event type was encountered");

        public record StorageExceptionError(Exception Exception) : ExceptionError("SOP_456", "There was an error with Dynamo", Exception);

        public record ExceptionError(string Code, string Message, Exception exception) : ErrorDetail(Code, Message);
    }
}
