using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Infrastructure.Services;

public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _queue;

    public EmailQueue()
    {
        // Unbounded channel for simplicity, but could be bounded in prod
        _queue = Channel.CreateUnbounded<EmailMessage>();
    }

    public ValueTask EnqueueEmailAsync(EmailMessage message)
    {
        return _queue.Writer.WriteAsync(message);
    }

    public ValueTask<EmailMessage> DequeueEmailAsync(CancellationToken ct)
    {
        return _queue.Reader.ReadAsync(ct);
    }
}
