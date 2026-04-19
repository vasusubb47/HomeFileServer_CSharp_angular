using System.Diagnostics;
using api.Services.BackgroundServices.EmailProcessingService;
using api.UtilityClass;

namespace api.Services.EmailService;

public class QueuedEmailService(EmailProcessingChannel channel) : IEmailService
{
    public async Task<bool> SendEmailAsync(EmailStructure emailStructure)
    {
        // Capture the current trace context from the API request thread
        var activity = Activity.Current;
        emailStructure.TraceId = activity?.TraceId.ToHexString();
        emailStructure.SpanId = activity?.SpanId.ToHexString();

        await channel.AddEmailAsync(emailStructure);
        return true; 
    }
}
