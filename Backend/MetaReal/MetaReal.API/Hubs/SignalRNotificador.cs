using MetaReal.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MetaReal.API.Hubs;

public class SignalRNotificador : INotificador
{
    private readonly IHubContext<NotificacoesHub> _hub;

    public SignalRNotificador(IHubContext<NotificacoesHub> hub)
    {
        _hub = hub;
    }

    public Task NotificarVendedor(Guid idVendedor, string tipo, string mensagem) =>
        _hub.Clients.Group($"vendedor-{idVendedor}").SendAsync("notificacao", new
        {
            tipo,
            mensagem,
            dataHora = DateTime.UtcNow
        });

    public Task NotificarGerentes(string tipo, string mensagem) =>
        _hub.Clients.Group("gerentes").SendAsync("notificacao", new
        {
            tipo,
            mensagem,
            dataHora = DateTime.UtcNow
        });
}
