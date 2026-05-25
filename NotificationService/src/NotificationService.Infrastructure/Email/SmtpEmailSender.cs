using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace NotificationService.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var smtpHost = _config["Smtp:Host"]!;
        var smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
        var smtpUser = _config["Smtp:Username"]!;
        var smtpPass = _config["Smtp:Password"]!;
        var fromEmail = _config["Smtp:FromEmail"]!;

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        using var message = new MailMessage(fromEmail, to, subject, body);
        await client.SendMailAsync(message, cancellationToken);

        _logger.LogInformation("Email sent to {To} with subject '{Subject}'", to, subject);
    }
}
