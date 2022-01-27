namespace Utilities;

public class Result<TSuccess, TError>
{ }

public class SuccessResult<TSuccess, TError> : Result<TSuccess, TError>
{
    public TSuccess Some { get; init; }
}

public class ErrorResult<TSuccess, TError> : Result<TSuccess, TError>
{
    public TError None { get; init; }
}
