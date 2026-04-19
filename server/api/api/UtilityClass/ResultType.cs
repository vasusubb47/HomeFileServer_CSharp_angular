namespace api.UtilityClass;

public class ResultType<T>
{
    public string DataFrom { get; set; } = string.Empty;
    public T Data { get; set; }
}
