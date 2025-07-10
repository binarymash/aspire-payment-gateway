using System.Diagnostics;

namespace BinaryMash.Extensions.Results;

/// <summary>
/// Result extensions for integrating with System.Diagnostics.Activity
/// </summary>
public static class ResultActivityExtensions
{
    /// <summary>
    /// Sets the current activity status based on the result status and invokes the provided action on success.
    /// This should onlty be used at the end of an activity, on the final result of the activity.
    /// </summary>
    /// <remarks>
    /// On success invokes the <cref="successAction"/> and sets the current qctivity status to Ok.
    /// On failure, sets the curretn activity status to Error.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="onSuccess">Optional action to invoke when the result is Success</param>
    /// <param name="onFailure">Optional action to invoke when the result is Failure</param>
    /// <returns></returns>
    public static Result<T> RecordActivityResult<T>(this Result<T> result, Action<Result<T>>? onSuccess = null, Action<Result<T>>? onFailure = null)
    {
        return result.Match(
            onSuccess: success =>
            {
                onSuccess?.Invoke(result);

                Activity.Current?.SetStatus(ActivityStatusCode.Ok);
                return success;
            },
            onFailure: failure =>
            {
                onFailure?.Invoke(result);

                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                Activity.Current?.SetTag("ErrorCode", failure.ErrorDetail.Code);
                return failure;
            });
    }

    /// <summary>
    /// Sets the current activity status based on the result status and invokes the provided action on success.
    /// This should onlty be used at the end of an activity, on the final result of the activity.
    /// </summary>
    /// <remarks>
    /// On success invokes the <cref="successAction"/> and sets the current qctivity status to Ok.
    /// On failure, sets the curretn activity status to Error.
    /// </remarks>
    /// <param name="result"></param>
    /// <param name="onSuccess">Optional action to invoke when the result is Success</param>
    /// <param name="onFailure">Optional action to invoke when the result is Failure</param>
    /// <returns></returns>
    public static Result RecordActivityResult(this Result result, Action<Result>? onSuccess = null, Action<Result>? onFailure = null)
    {
        return result.Match(
            onSuccess: success =>
            {
                onSuccess?.Invoke(result);
                return success;
            },
            onFailure: failure =>
            {
                onFailure?.Invoke(result);
                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                Activity.Current?.SetTag("ErrorCode", failure.ErrorDetail.Code);
                return failure;
            });
    }
}
