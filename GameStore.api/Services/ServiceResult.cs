namespace GameStore.api;

public enum ServiceErrorType
{
    NotFound,
    ValidationError,
    Conflict,
    Forbidden
}

public class ServiceResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public ServiceErrorType? ErrorType { get; }

    protected ServiceResult(bool isSuccess, string? errorMessage, ServiceErrorType? errorType)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static ServiceResult Success() => new(true, null, null);
    public static ServiceResult NotFound(string message) => new(false, message, ServiceErrorType.NotFound);
    public static ServiceResult ValidationError(string message) => new(false, message, ServiceErrorType.ValidationError);
    public static ServiceResult Conflict(string message) => new(false, message, ServiceErrorType.Conflict);
    public static ServiceResult Forbidden(string message) => new(false, message, ServiceErrorType.Forbidden);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }

    private ServiceResult(bool isSuccess, T? data, string? errorMessage, ServiceErrorType? errorType)
        : base(isSuccess, errorMessage, errorType)
    {
        Data = data;
    }

    public static ServiceResult<T> Success(T data) => new(true, data, null, null);
    public new static ServiceResult<T> NotFound(string message) => new(false, default, message, ServiceErrorType.NotFound);
    public new static ServiceResult<T> ValidationError(string message) => new(false, default, message, ServiceErrorType.ValidationError);
    public new static ServiceResult<T> Conflict(string message) => new(false, default, message, ServiceErrorType.Conflict);
    public new static ServiceResult<T> Forbidden(string message) => new(false, default, message, ServiceErrorType.Forbidden);
}
