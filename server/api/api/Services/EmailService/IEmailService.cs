using api.UtilityClass;

namespace api.Services.EmailService;

public interface IEmailService
{
    public Task<bool> SendEmailAsync(EmailStructure emailStructure);
}
