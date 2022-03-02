namespace Core.Types;

public record Result<TSuccess, TError>();

public record Some<TSuccess, TError>(TSuccess Result) : Result<TSuccess, TError>;

public record None<TSuccess, TError>(TError Error) : Result<TSuccess, TError>;
