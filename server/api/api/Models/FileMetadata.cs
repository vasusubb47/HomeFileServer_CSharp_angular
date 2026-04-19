using LinqToDB;
using LinqToDB.Mapping;

namespace api.Models;

internal interface IFileMetadata : IFileHash
{
    string FileName { get; set; }
    long FileSize { get; set; }
    string FileType { get; set; }
}

internal interface IFileHash
{
    List<string>? FileHashes { get; set; }
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
    public long FileSize { get; set; } = 0;
    
    [Column(Name = "file_type", DataType = DataType.VarChar, Length = 255), NotNull]
    public string FileType { get; set; } = string.Empty;
    
    // To store "hashType|hashValue" strings in a Postgres Array
    [Column(Name = "file_hashes", DbType = "text[]", Length = 125)]
    public List<string>? FileHashes { get; set; } = [];

    [Column(Name = "created_at", SkipOnInsert = true)]
    public DateTimeOffset CreatedAt { get; set; }

    [Column(Name = "updated_at", SkipOnInsert = true), Nullable]
    public DateTimeOffset? UpdatedAt { get; set; } = null;
    
    [Association(ThisKey = nameof(FileId), OtherKey = nameof(File.FileId), CanBeNull = false)]
    public UserFile File { get; set; }
}
