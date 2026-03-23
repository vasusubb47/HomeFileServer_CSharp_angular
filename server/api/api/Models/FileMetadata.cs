using LinqToDB;
using LinqToDB.Mapping;

namespace api.Models;

internal interface IFileMetadata
{
    string FileName { get; set; }
    int FileSize { get; set; }
    string FileType { get; set; }
    List<string> FileHashes { get; set; }
}

[Table(Name = "file_metadata")]
public class FileMetadata: IFileId, IFileMetadata, IModelTimeInfo
{
    [PrimaryKey]
    [Column(Name = "file_id"), NotNull]
    public Guid FileId { get; set; } = Guid.Empty;
    
    [Column(Name = "file_name", DataType = DataType.VarChar, Length = 125), NotNull]
    public string FileName { get; set; } = string.Empty;
    
    [Column(Name = "file_size"), NotNull]
    public int FileSize { get; set; } = 0;
    
    [Column(Name = "file_type", DataType = DataType.VarChar, Length = 255), NotNull]
    public string FileType { get; set; } = string.Empty;
    
    // To store "hashType|hashValue" strings in a Postgres Array
    [Column(Name = "file_hashes", DbType = "text[]", Length = 125), NotNull]
    public List<string> FileHashes { get; set; } = new List<string>();

    [Column(Name = "created_at", SkipOnInsert = true)]
    public DateTimeOffset CreatedAt { get; set; }

    [Column(Name = "updated_at", SkipOnInsert = true), Nullable]
    public DateTimeOffset? UpdatedAt { get; set; } = null;
    
    [Association(ThisKey = nameof(FileId), OtherKey = nameof(File.FileId), CanBeNull = false)]
    public UserFile File { get; set; }
}
