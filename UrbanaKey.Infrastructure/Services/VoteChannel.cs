using System.Threading.Channels;
using System.Threading.Tasks;
using UrbanaKey.Core.Features.Assemblies;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Infrastructure.Services;

public class VoteChannel : IVoteChannel
{
    private readonly Channel<VoteDto> _channel;

    public VoteChannel()
    {
        _channel = Channel.CreateUnbounded<VoteDto>();
    }

    public ValueTask WriteVoteAsync(VoteDto vote)
    {
        return _channel.Writer.WriteAsync(vote);
    }

    public ChannelReader<VoteDto> Reader => _channel.Reader;
}
