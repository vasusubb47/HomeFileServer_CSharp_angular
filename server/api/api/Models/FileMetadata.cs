namespace api.Models;

internal interface IFileMetadata
{
    string FileName { get; set; }
    int FileSize { get; set; }
    string FileType { get; set; }
    string FileHash { get; set; }
}

public class FileMetadata: IFileId, IFileMetadata, IModelTimeInfo
{
    public Guid FileId { get; set; } = Guid.Empty;
    public string FileName { get; set; } = string.Empty;
    public int FileSize { get; set; } = 0;
    public string FileType { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; } = null;
}
