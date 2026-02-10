using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Infrastructure.Services;

public class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendEmailAsync(EmailMessage message)
    {
        var smtpConfig = configuration.GetSection("EmailSettings");
        
        // Ensure config exists to avoid runtime errors, though in prod we'd validate startup
        var host = smtpConfig["Host"];
        if (string.IsNullOrEmpty(host)) return; // Or throw/log

        using var client = new SmtpClient(host)
        {
            Port = int.Parse(smtpConfig["Port"] ?? "587"),
            Credentials = new NetworkCredential(smtpConfig["User"], smtpConfig["Password"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpConfig["From"] ?? "noreply@urbanakey.com"),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml,
        };
        mailMessage.To.Add(message.To);

        await client.SendMailAsync(mailMessage);
    }
}
