using careerhive.application.Interfaces.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;

namespace careerhive.application.Service;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
        var smtpUser = _configuration["EmailSettings:SmtpUser"];
        var smtpPass = _configuration["EmailSettings:SmtpPass"];
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("Career Hive", smtpUser));
        mimeMessage.To.Add(new MailboxAddress("", email));
        mimeMessage.Subject = subject;
        mimeMessage.Body = new TextPart("html") { Text = message };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Email to {email} sent successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send email to {email}: {ex.Message}");
        }
    }
}
