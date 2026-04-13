using CSharpFunctionalExtensions;

namespace ChessTournaments.Modules.Tournaments.Domain.Common;

public static class ResultExtensions
{
    public static Result ToResult(this Error error)
    {
        return Result.Failure(error.Message);
    }

    public static Result<T> ToResult<T>(this Error error)
    {
        return Result.Failure<T>(error.Message);
    }

    public static Error? GetError(this Result result)
    {
        if (result.IsSuccess)
            return null;

        return new Error("Error", result.Error);
    }

    public static Error? GetError<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return null;

        return new Error("Error", result.Error);
    }
}
