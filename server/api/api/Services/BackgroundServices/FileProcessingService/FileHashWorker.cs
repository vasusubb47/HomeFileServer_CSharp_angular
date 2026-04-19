using api.Services.DatabaseService;
using api.UtilityClass;
using LinqToDB;

namespace api.Services.BackgroundServices.FileProcessingService;

public class FileHashWorker(
    IServiceProvider serviceProvider, 
    FileProcessingChannel channel,
    ILogger<FileHashWorker> logger,
    AppSettings settings
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in channel.ReadAllAsync(stoppingToken))
        {
            logger.LogInformation("hashing the file {FileId}", job.FileId);
            using var scope = serviceProvider.CreateScope();
            var fileService = scope.ServiceProvider.GetRequiredService<FileService>();
            var db = scope.ServiceProvider.GetRequiredService<IDbService>();

            // 1. Get the stream using the method we wrote earlier
            // (You'll need to fetch the metadata first to get the extension/ids)
            await using Stream stream = await fileService.GetFileStreamAsync(job, job.FileExtenstion);
            
            {
                // 2. Calculate the Hash
                string hash = await AppUtility.CalculateSha256HashFromStreamAsync(stream);

                // 3. Update the Database
                await db.FilesMetadata
                    .Where(m => m.FileId == job.FileId)
                    .Set(m => m.FileHashes, [$"Sha256|{hash}"])
                    .UpdateAsync(stoppingToken);
                logger.LogInformation("hashed {FileId} file Hash {FileHash}", job.FileId, hash);
            }
        }
    }
}
