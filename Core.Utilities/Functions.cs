using Core.Types;

namespace Core.Utilities;

public static class FileOps
{
    public static async Task WriteFileAsync(string filename, string body)
    {
        await using var writer = File.CreateText(filename);
        await writer.WriteLineAsync(body);
    }
}

public static class Ops
{
    public static async Task<Result<TResult, Exception>> RunCatchingAsync<TResult>(Func<Task<TResult>> func)
    {
        try
        {
            return new SuccessResult<TResult, Exception>(await func());
        }
        catch (Exception e)
        {
            return new ErrorResult<TResult, Exception>(e);
        }
    }

    public static async Task<Result<bool, Exception>> RunCatchingAsync(Func<Task> func)
    {
        try
        {
            await func();
            return new SuccessResult<bool, Exception>(true);
        }
        catch (Exception e)
        {
            return new ErrorResult<bool, Exception>(e);
        }
    }

    public static Result<bool, Exception> RunCatching(Action func)
    {
        try
        {
            func();
            return new SuccessResult<bool, Exception>(true);
        }
        catch (Exception e)
        {
            return new ErrorResult<bool, Exception>(e);
        }
    }

    public static Result<TResult, Exception> RunCatching<TResult>(Func<TResult> func)
    {
        try
        {
            return new SuccessResult<TResult, Exception>(func());
        }
        catch (Exception e)
        {
            return new ErrorResult<TResult, Exception>(e);
        }
    }
}
