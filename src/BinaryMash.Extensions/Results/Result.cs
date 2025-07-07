namespace BinaryMash.Extensions.Results;

public abstract record Result<T>
{
    public bool IsSuccess { get; protected set; }

    public ErrorDetail ErrorDetail { get; protected set; }

    public bool IsFailure => !IsSuccess;

    public static Result<TValue> Success<TValue>(TValue value) => new SuccessResult<TValue>(value);

    public static Result<TValue> Failure<TValue>(ErrorDetail errorDetail) => new FailureResult<TValue>(errorDetail);

    public T Value { get; protected set; } = default!;

    protected Result(bool isSuccess, T value = default!, ErrorDetail errorDetail = default!)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorDetail = errorDetail;
    }

    public static implicit operator Result<T>(T value) => new SuccessResult<T>(value);

    public Result<TOut> Then<TOut>(Func<Result<T>, Result<TOut>> then)
    {
        if (IsSuccess)
        {
            return then(this);
        }

        return Failure<TOut>(ErrorDetail);
    }

    public TOut Match<TOut>(Func<Result<T>, TOut> onSuccess, Func<Result<T>, TOut> onFailure)
    {
        if (IsSuccess)
        {
            return onSuccess(this);
        }

        return onFailure(this);
    }
}

public abstract record Result : Result<Unit>
{
    protected Result(bool isSuccess, ErrorDetail errorDetail = null!) : base(isSuccess, new Unit(), errorDetail)
    { }

    public static Result Success => new SuccessResult();

    public static Result Failure(ErrorDetail errorDetail) => new FailureResult(errorDetail);

    public TOut Match<TOut>(Func<Result, TOut> onSuccess, Func<Result, TOut> onFailure)
    {
        if (IsSuccess)
        {
            return onSuccess(this);
        }

        return onFailure(this);
    }
}

public record SuccessResult<T> : Result<T>
{
    public SuccessResult(T value) : base(true, value) { }
}

public record FailureResult<T> : Result<T>
{
    public FailureResult(ErrorDetail errorDetail) : base(false, default!, errorDetail) { }
}

public record SuccessResult : Result
{
    public SuccessResult() : base(true) { }
}

public record FailureResult : Result
{
    public FailureResult(ErrorDetail errorDetail) : base(false, errorDetail) { }
}

public record Unit();

