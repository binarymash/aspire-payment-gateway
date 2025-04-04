namespace AspirePaymentGateway.Api.Extensions.Results
{
    public abstract record Result
    {
        public bool IsSuccess { get; protected set; }
        
        public ErrorDetail ErrorDetail { get; protected set; }

        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, ErrorDetail errorDetail = null!)
        {
            IsSuccess = isSuccess;
            ErrorDetail = errorDetail;
        }
    }

    public abstract record Result<T> : Result
    {
        public T Value { get; protected set; } = default!;

        protected Result(bool isSuccess, T value = default!, ErrorDetail errorDetail = default!) : base(isSuccess, errorDetail)
        {
            Value = value;
        }

        public static implicit operator Result<T>(T value) => new OKResult<T>(value);
    }

    public record OKResult : Result
    {
        public OKResult() : base(true) { }
    }

    public record ErrorResult : Result
    {
        public ErrorResult(ErrorDetail errorDetail) : base(false)
        {
        }
    }

    public record OKResult<T> : Result<T>
    {
        public OKResult(T value) : base(true, value) { }
    }

    public record ErrorResult<T> : Result<T>
    {
        public ErrorResult(ErrorDetail errorDetail) : base(false, default!, errorDetail) { }
    }
}
