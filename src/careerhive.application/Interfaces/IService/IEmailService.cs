namespace careerhive.application.Interfaces.IService;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}
