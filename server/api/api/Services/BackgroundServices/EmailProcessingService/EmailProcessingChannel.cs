using System.Threading.Channels;
using api.Services.EmailService;
using api.UtilityClass;

namespace api.Services.BackgroundServices.EmailProcessingService;

public class EmailProcessingChannel
{
    private readonly Channel<EmailStructure> _channel;
    private readonly ILogger<EmailProcessingChannel> _logger;

    public EmailProcessingChannel(ILogger<EmailProcessingChannel> logger)
    {
        _logger = logger;
        // Bounded means we limit the queue size to avoid memory overflow
        var options = new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.Wait };
        _channel = Channel.CreateBounded<EmailStructure>(options);
    }

    public async ValueTask AddEmailAsync(EmailStructure emailStructure)
    {
        _logger.LogInformation("Adding file {EmailId}", emailStructure.EmailId);
        await _channel.Writer.WriteAsync(emailStructure);
    }

    public IAsyncEnumerable<EmailStructure> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}
