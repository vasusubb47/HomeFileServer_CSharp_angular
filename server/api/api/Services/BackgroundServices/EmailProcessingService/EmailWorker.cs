using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using api.UtilityClass;

namespace api.Services.BackgroundServices.EmailProcessingService;

public class EmailWorker(
    EmailProcessingChannel channel,
    AppSettings appSettings,
    ILogger<EmailWorker> logger)
    : BackgroundService
{
    // Name this specifically for OpenTelemetry registration
    private static readonly ActivitySource ActivitySource = new("EmailService.Worker");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email Background Worker started.");

        await foreach (var email in channel.ReadAllAsync(stoppingToken))
        {
            await ProcessEmailWithTraceAsync(email, stoppingToken);
        }
    }

    private async Task ProcessEmailWithTraceAsync(EmailStructure email, CancellationToken ct)
    {
        // 1. Reconstruct Parent Context from stored IDs
        var parentContext = default(ActivityContext);
        if (!string.IsNullOrEmpty(email.TraceId) && !string.IsNullOrEmpty(email.SpanId))
        {
            parentContext = new ActivityContext(
                ActivityTraceId.CreateFromString(email.TraceId),
                ActivitySpanId.CreateFromString(email.SpanId),
                ActivityTraceFlags.Recorded);
        }

        // 2. Start a New Activity linked to the Parent
        using var activity = ActivitySource.StartActivity(
            "EmailWorker.SendEmail", 
            ActivityKind.Consumer, 
            parentContext);

        // Add metadata to the trace for easier searching
        activity?.SetTag("email.id", email.EmailId);
        activity?.SetTag("email.recipient", email.ToEmail);

        try
        {
            logger.LogInformation("Processing email {EmailId}. TraceID preserved: {TraceId}", 
                email.EmailId, Activity.Current?.TraceId);

            if (!appSettings.Email.SendEmail)
            {
                logger.LogWarning("Email sending is disabled in settings.");
                return;
            }

            await SendEmailViaSmtpAsync(email, ct);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email {EmailId} in background task.", email.EmailId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
    }

    private async Task SendEmailViaSmtpAsync(EmailStructure email, CancellationToken ct)
    {
        var targetEmail = appSettings.IsProd ? email.ToEmail : appSettings.Email.To;
        logger.LogInformation("Sending email {EmailId} to {ToEmail}. TraceID: {TraceId}", email.EmailId, targetEmail, Activity.Current?.TraceId);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Hobby Project", appSettings.Email.From));
        message.To.Add(new MailboxAddress("Recipient", targetEmail));
        message.Subject = email.Subject;

        message.Body = email.IsHtml 
            ? new TextPart("html") { Text = email.Body } 
            : new TextPart("text") { Text = email.Body };

        using var client = new SmtpClient();
        
        await client.ConnectAsync(appSettings.Email.SmtpHost, appSettings.Email.SmtpPort, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(appSettings.Email.From, appSettings.Email.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
        logger.LogInformation("Email sent to {ToEmail}.", targetEmail);
    }
}
