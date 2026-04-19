namespace api.UtilityClass;

public class EmailStructure
{
    public Guid EmailId { get; set; } = Guid.NewGuid();
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = false;

    // Add this to store the trace context
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
}
