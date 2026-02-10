using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace UrbanaKey.Infrastructure.Hubs;

public class AssemblyHub : Hub
{
    public async Task JoinAssembly(string assemblyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, assemblyId);
    }
}
