namespace AccountService.Results;

public class Result<T>
{
    private Result(T value, bool isSuccess, string error, int statusCode)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    public int StatusCode { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value, true, null!, 200);
    }

    public static Result<T> Success(T value, int statusCode)
    {
        return new Result<T>(value, true, null!, statusCode);
    }

    public static Result<T> Failure(string error, int statusCode = 400)
    {
        return new Result<T>(default!, false, error, statusCode);
    }
}