using System.ComponentModel;
using System.Net.Http.Headers;
namespace AccountsUpdate.Domain.Common;

public readonly struct Result<TValue,TError>
{
    public bool IsError { get; }

    private readonly TValue? _value;
    private readonly TError? _error;
    public Result(TValue value)
    {
        IsError = false;
        _value = value;
        _error = default;
    }
    private Result(TError error)
    {
        IsError = true;
        _value = default;
        _error = error;
    }
    public bool IsSuccess => !IsError;
    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<TError, TResult> failure) =>
        !IsError ? success(_value!) : failure(_error!);
}
