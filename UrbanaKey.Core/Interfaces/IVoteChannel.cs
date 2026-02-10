using System.Threading.Tasks;
using UrbanaKey.Core.Features.Assemblies;

namespace UrbanaKey.Core.Interfaces;

public interface IVoteChannel
{
    ValueTask WriteVoteAsync(VoteDto vote);
}
