using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Infrastructure.Hubs;

public class PanicHub : Hub
{
    private readonly ITenantProvider _tenantProvider;

    public PanicHub(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override async Task OnConnectedAsync()
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (tenantId != Guid.Empty)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, tenantId.ToString());
        }

        await base.OnConnectedAsync();
    }

    public async Task TriggerPanic(string unitIdentifier)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (tenantId != Guid.Empty)
        {
            await Clients.Group(tenantId.ToString()).SendAsync("PanicAlert", new 
            { 
                Unit = unitIdentifier, 
                Timestamp = DateTime.UtcNow,
                Message = "ALERTA DE PÁNICO ACTIVA"
            });
        }
    }
}
