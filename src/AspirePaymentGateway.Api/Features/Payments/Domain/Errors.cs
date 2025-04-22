using AspirePaymentGateway.Api.Extensions.Results;
using FluentValidation.Results;

namespace AspirePaymentGateway.Api.Features.Payments.Domain
{
    public static class Errors
    {
        public static class Codes
        {
            //1xx Payment API
            public const string SOP101ValidationError = "SOP_101";
            public const string SOP102PaymentNotFound = "SOP_102";

            //2xx Payment Internal Errors
            public const string SOP201UnknownEventType = "SOP_201";

            //3xx Storage Errors
            public const string SOP301StorageReadError = "SOP_301";
            public const string SOP302StorageWriteError = "SOP_302";

            //4xx Fraud API
            public const string SOP401FraudApiError = "SOP_401";

            //5xx Bank API
            public const string SOP501BankApiError = "SOP_501";
        }


        public record ValidationError(ValidationResult ValidationResult) : ErrorDetail(Codes.SOP101ValidationError, "There was a validation error");

        public record PaymentNotFoundError() : ErrorDetail(Codes.SOP102PaymentNotFound, "The payment was not found");

        public record UnknownEventTypeError(string EventType) : ErrorDetail(Codes.SOP201UnknownEventType, "An unknown event type was encountered");

        public record StorageReadExceptionError(Exception Exception) : ExceptionError(Codes.SOP301StorageReadError, "There was an error reading from storage", Exception);

        public record StorageWriteExceptionError(Exception Exception) : ExceptionError(Codes.SOP302StorageWriteError, "There was an error writing to storage", Exception);

        public record FraudApiExceptionError(Exception Exception) : ExceptionError(Codes.SOP401FraudApiError, "There was an error with the fraud API", Exception);

        public record BankApiExceptionError(Exception Exception) : ExceptionError(Codes.SOP501BankApiError, "There was an error with the bank API", Exception);

        public abstract record ExceptionError(string Code, string Message, Exception Exception) : ErrorDetail(Code, Message);
    }
}
