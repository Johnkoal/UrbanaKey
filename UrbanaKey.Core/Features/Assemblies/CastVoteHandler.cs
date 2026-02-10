using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Assemblies;

public record CastVoteCommand(VoteDto Vote) : IRequest;

public class CastVoteHandler : IRequestHandler<CastVoteCommand>
{
    private readonly IVoteChannel _voteChannel;
    private readonly ITenantProvider _tenantProvider;

    public CastVoteHandler(IVoteChannel voteChannel, ITenantProvider tenantProvider)
    {
        _voteChannel = voteChannel;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(CastVoteCommand request, CancellationToken cancellationToken)
    {
        // Populate TenantId
        request.Vote.TenantId = _tenantProvider.GetTenantId();

        // 2. Write to Channel (Non-blocking DB write)
        await _voteChannel.WriteVoteAsync(request.Vote);
    }
}
