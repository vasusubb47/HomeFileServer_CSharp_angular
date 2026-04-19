using System.Threading.Channels;
using api.Models;

namespace api.Services.BackgroundServices.FileProcessingService;

public class FileProcessingChannel
{
    private readonly Channel<UserFileNameExt> _channel;
    private readonly ILogger<FileProcessingChannel> _logger;

    public FileProcessingChannel(ILogger<FileProcessingChannel> logger)
    {
        _logger = logger;
        // Bounded means we limit the queue size to avoid memory overflow
        var options = new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.Wait };
        _channel = Channel.CreateBounded<UserFileNameExt>(options);
    }

    public async ValueTask AddFileAsync(UserFileNameExt userFileNameExt)
    {
        _logger.LogDebug("Adding file {FileId}", userFileNameExt.FileId);
        await _channel.Writer.WriteAsync(userFileNameExt);
    }

    public IAsyncEnumerable<UserFileNameExt> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}
