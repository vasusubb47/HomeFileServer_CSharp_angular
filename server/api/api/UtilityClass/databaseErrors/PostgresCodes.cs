namespace api.UtilityClass.databaseErrors;

public record DatabaseError(string Code, string Description);

public static class PostgresCodes
{
    // 23xxx: Constraint Violations
    public const string UniqueViolation = "23505";
    public const string ForeignKeyViolation = "23503";
    public const string NotNullViolation = "23502";
    public const string CheckViolation = "23514";

    // 42xxx: Syntax/Structure Errors
    public const string UndefinedTable = "42P01";
    public const string UndefinedColumn = "42703";

    // 08xxx: Connection Errors
    public const string ConnectionFailure = "08006";
}

// 409 Conflict
public record UniqueViolationError(string Message) : IDatabaseError
{
    public string Code => PostgresCodes.UniqueViolation;
    public int StatusCode => 409;
}

// 404 Not Found (or 400 depending on your API design)
public record ForeignKeyError(string Message) : IDatabaseError
{
    public string Code => PostgresCodes.ForeignKeyViolation;
    public int StatusCode => 404;
}

// 400 Bad Request
public record ValidationError(string Code, string Message) : IDatabaseError
{
    public int StatusCode => 400;
}

// 503 Service Unavailable
public record ConnectionError(string Message) : IDatabaseError
{
    public string Code => PostgresCodes.ConnectionFailure;
    public int StatusCode => 503;
}

// 500 Internal Server Error
public record GeneralDatabaseError(string Message, string InternalCode = "00000") : IDatabaseError
{
    public string Code => InternalCode;
    public int StatusCode => 500;
}
