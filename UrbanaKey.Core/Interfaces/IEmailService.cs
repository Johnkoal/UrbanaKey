using System.Threading.Tasks;

namespace UrbanaKey.Core.Interfaces;

public record EmailMessage(string To, string Subject, string Body, bool IsHtml = true);

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message);
}
