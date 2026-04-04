namespace api.UtilityClass;

public class Result<TValue, TError>
{
    public TValue? Value { get; }
    public TError? Error { get; }
    public bool IsSuccess { get; }

    private Result(TValue value) { Value = value; IsSuccess = true; }
    private Result(TError error) { Error = error; IsSuccess = false; }

    public static Result<TValue, TError> Success(TValue value) => new(value);
    public static Result<TValue, TError> Failure(TError error) => new(error);
}
