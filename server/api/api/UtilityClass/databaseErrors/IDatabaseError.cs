namespace api.UtilityClass.databaseErrors;

public interface IDatabaseError : IHttpStatusCode
{
    string Code { get; }
    string Message { get; }
}

public interface IHttpStatusCode
{
    int StatusCode { get; }
}
