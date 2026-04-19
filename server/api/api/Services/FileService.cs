using api.Models;

namespace api.Services;

public class FileService(
    AppSettings appSettings,
    ILogger<FileService> logger
)
{
    public async Task<bool> SaveFileAsync(UserBucketFileId userBucketFileId, IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName); // e.g., .jpg or .pdf
        var fileName = $"{userBucketFileId.FileId}{extension}";

        // 2. Build the path: base/userid/bucketid
        // Path.Combine handles the slashes for Windows/Linux automatically
        var targetFolder = Path.Combine(
            appSettings.FileSettings.BaseFilePath, 
            userBucketFileId.UserId.ToString(), 
            userBucketFileId.BucketId.ToString()
        );

        // 3. Create the directories if they don't exist
        if (!Directory.Exists(targetFolder))
        {
            // This creates all nested folders in the path at once
            Directory.CreateDirectory(targetFolder);
        }

        // 4. Combine folder and filename
        var fullPath = Path.Combine(targetFolder, fileName);

        // 5. Save the file stream
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return true;
    }

    public async Task<Stream> GetFileStreamAsync<T>(T id, string extension)
    where T : IUserId, IBucketId, IFileId
    {
        // 1. Reconstruct the folder path
        var targetFolder = Path.Combine(
            appSettings.FileSettings.BaseFilePath, 
            id.UserId.ToString(), 
            id.BucketId.ToString()
        );

        // 2. Reconstruct the exact filename
        var fileName = $"{id.FileId}{extension}";
        var fullPath = Path.Combine(targetFolder, fileName);

        // 3. Check if file exists
        if (!File.Exists(fullPath))
        {
            return null; // Or throw a FileNotFoundException
        }

        // 4. Return the stream for reading
        // Note: We use FileOptions.Asynchronous for better performance in Core
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
    }
}
