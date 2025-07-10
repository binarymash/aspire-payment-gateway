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

    /// <summary>
    /// ,If the previous result is a success, invokes a function that explivitly returns a Result of type TOut.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="then"></param>
    /// <returns></returns>
    public Result<TOut> Then<TOut>(Func<Result<T>, Result<TOut>> then)
    {
        if (IsSuccess)
        {
            return then(this);
        }

        return Failure<TOut>(ErrorDetail);
    }

    /// <summary>
    /// If the previous result is a success, invokes an action. The original result is then returned
    /// </summary>
    /// <param name="then">The action to invoke when the original result is a success</param>
    /// <returns>The original result</returns>
    public Result<T> Then(Action<Result<T>> then)
    {
        if (IsSuccess)
        {
            then.Invoke(this);
            return this;
        }

        return this;
    }

    public async Task<Result<TOut>> ThenAsync<TOut>(Func<Result<T>, Task<Result<TOut>>> then)
    {
        if (IsSuccess)
        {
            return await then(this);
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

#pragma warning disable S2094 // Classes should not be empty
public record Unit();
#pragma warning restore S2094 // Classes should not be empty

