namespace AspirePaymentGateway.Api.Extensions.Results
{
    public abstract record Result
    {
        public bool IsSuccess { get; protected set; }
        
        public ErrorDetail ErrorDetail { get; protected set; }

        public bool IsFailure => !IsSuccess;

        public static Result Ok => new OkResult();

        public static Result Error(ErrorDetail errorDetail) => new ErrorResult(errorDetail);

        public static Result<T> Error<T>(ErrorDetail errorDetail) => new ErrorResult<T>(errorDetail);

        protected Result(bool isSuccess, ErrorDetail errorDetail = null!)
        {
            IsSuccess = isSuccess;
            ErrorDetail = errorDetail;
        }
    }

    public record OkResult : Result
    {
        public OkResult() : base(true) { }
    }

    public record ErrorResult : Result
    {
        public ErrorResult(ErrorDetail errorDetail) : base(false)
        {
            ErrorDetail = errorDetail;
        }
    }

    public abstract record Result<T> : Result
    {
        public T Value { get; protected set; } = default!;

        protected Result(bool isSuccess, T value = default!, ErrorDetail errorDetail = default!) 
            : base(isSuccess, errorDetail)
        {
            Value = value;
        }

        public static implicit operator Result<T>(T value) => new OkResult<T>(value);
    }

    public record OkResult<T> : Result<T>
    {
        public OkResult(T value) : base(true, value) { }
    }

    public record ErrorResult<T> : Result<T>
    {
        public ErrorResult(ErrorDetail errorDetail) : base(false, default!, errorDetail) { }
    }
}
