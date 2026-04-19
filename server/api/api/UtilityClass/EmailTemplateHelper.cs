namespace api.UtilityClass;

public static class EmailTemplateHelper
{
    private static readonly string TemplateFolder = Path.Combine(AppContext.BaseDirectory, "EmailTemplates");

    public static async Task<string> GetTemplateAsync(string templateName)
    {
        // Construct the full path
        var filePath = Path.Combine(TemplateFolder, $"{templateName}.html");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Email template '{templateName}' not found at {filePath}");
        }

        // Read the file content
        return await File.ReadAllTextAsync(filePath);
    }

    /// <summary>
    /// Pulls a template and replaces placeholders defined as {{Key}}
    /// </summary>
    public static async Task<string> GetTemplateWithReplacementsAsync(string templateName, Dictionary<string, string> replacements)
    {
        string content = await GetTemplateAsync(templateName);

        foreach (var item in replacements)
        {
            content = content.Replace($"{{{{{item.Key}}}}}", item.Value);
        }

        return content;
    }
}
