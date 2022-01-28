namespace Utilities;

public record Result<TSuccess, TError>();

public record SuccessResult<TSuccess, TError>(TSuccess Some) : Result<TSuccess, TError>;

public record ErrorResult<TSuccess, TError>(TError None) : Result<TSuccess, TError>;
