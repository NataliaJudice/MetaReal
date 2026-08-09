using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MetaReal.API.Hubs;

[Authorize]
public class NotificacoesHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var usuarioId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var vendedorId = Context.User?.FindFirstValue("vendedorId");
        var role = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (!string.IsNullOrEmpty(usuarioId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"usuario-{usuarioId}");
        }

        if (!string.IsNullOrEmpty(vendedorId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"vendedor-{vendedorId}");
        }

        if (role == "Gerente")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "gerentes");
        }

        await base.OnConnectedAsync();
    }
}
