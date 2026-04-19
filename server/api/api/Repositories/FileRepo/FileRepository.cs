using api.Models;
using api.Services;
using api.Services.BackgroundServices.FileProcessingService;
using api.Services.DatabaseService;
using LinqToDB;

namespace api.Repositories.FileRepo;

public class FileRepository(
    IDbService db,
    ILogger<FileRepository> logger,
    FileService fileService,
    FileProcessingChannel fileProcessingChannel
) : IFileRepository
{   
    public async Task<Guid> InsertFile(UserBucketId userBucketId, IFormFile file)
    {
        // Insert into user file to get a file id from db so we can insert file metadata with file id 
        var fileId = await db.UserFiles
            .Value(f => f.UserId, userBucketId.UserId)
            .Value(f => f.BucketId, userBucketId.BucketId)
            .Value(f => f.IsPublic, false)
            .InsertWithOutputAsync(f => f.FileId);
        
        // get basic info and save the file to db and into server location
        var fileInfo = new InsertUserFile
        {
            FileId = fileId,
            FileName = file.FileName,
            FileSize = file.Length,
            FileType = file.ContentType,
            FileHashes = null
        };

        var userBucketFileId = new UserBucketFileId
        {
            UserId = userBucketId.UserId,
            BucketId = userBucketId.BucketId,
            FileId = fileId
        };

        var fileMetadataTask = db.FilesMetadata
            .Value(fm => fm.FileId, fileInfo.FileId)
            .Value(fm => fm.FileName, fileInfo.FileName)
            .Value(fm => fm.FileSize, fileInfo.FileSize)
            .Value(fm => fm.FileType, fileInfo.FileType)
            .Value(fm => fm.FileHashes, fileInfo.FileHashes)
            .InsertAsync();
        
        var fileSaveTask = fileService.SaveFileAsync(userBucketFileId, file);
        
        // This waits for both. If one fails, it throws an exception.
        // If both fail, it captures the exceptions.
        await Task.WhenAll(fileMetadataTask, fileSaveTask);
        
        // db will send inserted file id, send into file hash channel
        await fileProcessingChannel.AddFileAsync(new UserFileNameExt
        {
            UserId = userBucketId.UserId,
            BucketId = userBucketId.BucketId,
            FileId = fileId,
            FileExtenstion = Path.GetExtension(file.FileName),
        });
        
        return fileId;
    }
}