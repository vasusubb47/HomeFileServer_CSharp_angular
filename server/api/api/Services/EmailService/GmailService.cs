using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace api.Services.EmailService;

public class GmailService(AppSettings appSettings) : IEmailService
{
    private readonly AppSettings _appSettings = appSettings;

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml)
    {
        if (!_appSettings.Email.SendEmail)
        {
            return false;
        }
        
        toEmail = _appSettings.IsProd? toEmail : _appSettings.Email.To;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Hobby Project Test", _appSettings.Email.From));
        message.To.Add(new MailboxAddress("Recipient Email", toEmail));
        message.Subject = subject;

        message.Body = isHtml ? new TextPart("html") { Text = body } : new TextPart("text") { Text = body };

        using var client = new SmtpClient();
        var didSend = false;
        
        try
        {
            // 3. Connect (Async)
            // 587 is the standard port for STARTTLS
            await client.ConnectAsync(_appSettings.Email.SmtpHost, _appSettings.Email.SmtpPort, SecureSocketOptions.StartTls);

            // 4. Authenticate
            await client.AuthenticateAsync(_appSettings.Email.From, _appSettings.Email.Password);
            Console.WriteLine("Authentication successful!");

            // 5. Send and wait for the 'Sent' signal
            // This will throw an exception if Gmail rejects it
            string response = await client.SendAsync(message);

            // 6. Print the raw response from Gmail's server
            Console.WriteLine("\n--- SUCCESS ---");
            Console.WriteLine($"Server Response: {response}");
            didSend = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n--- ERROR ---");
            Console.WriteLine($"Failed to send email. Message: {ex.Message}");
        }
        finally
        {
            // 7. Gracefully disconnect and close
            Console.WriteLine("\nDisconnecting...");
            await client.DisconnectAsync(true);
            Console.WriteLine("Program finished.");
        }


        return didSend;
    }
}