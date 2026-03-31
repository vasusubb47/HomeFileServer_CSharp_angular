namespace api.Services.EmailService;

public interface IEmailService
{
    public Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml);
}
