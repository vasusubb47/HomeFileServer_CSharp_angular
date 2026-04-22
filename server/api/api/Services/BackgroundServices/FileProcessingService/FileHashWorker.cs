using System.Diagnostics;
using api.Models;
using api.Services.DatabaseService;
using api.UtilityClass;
using LinqToDB;

namespace api.Services.BackgroundServices.FileProcessingService;

public class FileHashWorker(
    IServiceProvider serviceProvider, 
    FileProcessingChannel channel,
    ILogger<FileHashWorker> logger
) : BackgroundService
{
    // Define the ActivitySource for this specific worker
    private static readonly ActivitySource ActivitySource = new("FileProcessing.Worker");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in channel.ReadAllAsync(stoppingToken))
        {
            await ProcessFileJobWithTraceAsync(job, stoppingToken);
        }
    }

    private async Task ProcessFileJobWithTraceAsync(UserFileNameExt job, CancellationToken ct)
    {
        // 1. Reconstruct the parent context from the TraceId/SpanId stored in the channel item
        var parentContext = default(ActivityContext);
        if (!string.IsNullOrEmpty(job.TraceId) && !string.IsNullOrEmpty(job.SpanId))
        {
            parentContext = new ActivityContext(
                ActivityTraceId.CreateFromString(job.TraceId),
                ActivitySpanId.CreateFromString(job.SpanId),
                ActivityTraceFlags.Recorded);
        }

        // 2. Start the Activity. Kind.Consumer indicates this is the end of a queue/handoff
        using var activity = ActivitySource.StartActivity(
            "FileHashWorker.CalculateHash", 
            ActivityKind.Consumer, 
            parentContext);

        // Add metadata tags to make searching in your Trace tool (Jaeger/Zipkin) easier
        activity?.SetTag("file.id", job.FileId);
        activity?.SetTag("file.extension", job.FileExtenstion);

        try
        {
            logger.LogInformation("Starting hash process for {FileId}", job.FileId);

            using var scope = serviceProvider.CreateScope();
            var fileService = scope.ServiceProvider.GetRequiredService<FileService>();
            var db = scope.ServiceProvider.GetRequiredService<IDbService>();

            await using Stream stream = await fileService.GetFileStreamAsync(job, job.FileExtenstion);
            
            string hash = await AppUtility.CalculateSha256HashFromStreamAsync(stream);

            await db.FilesMetadata
                .Where(m => m.FileId == job.FileId)
                .Set(m => m.FileHashes, [$"Sha256|{hash}"])
                .UpdateAsync(ct);

            logger.LogInformation("Successfully hashed {FileId}. Hash: {FileHash}", job.FileId, hash);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            // Record the exception in the trace
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Error while hashing file {FileId}", job.FileId);
        }
    }
}
