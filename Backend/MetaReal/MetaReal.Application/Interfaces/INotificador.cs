namespace MetaReal.Application.Interfaces
{
    
    public interface INotificador
    {
        Task NotificarVendedor(Guid idVendedor, string tipo, string mensagem);
        Task NotificarGerentes(string tipo, string mensagem);
    }
}
