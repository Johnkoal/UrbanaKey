using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Interfaces;

public interface IEmailQueue
{
    ValueTask EnqueueEmailAsync(EmailMessage message);
    ValueTask<EmailMessage> DequeueEmailAsync(CancellationToken ct);
}
