using MetaReal.Domain.Models;

namespace MetaReal.Application.Interfaces
{
    public interface IUsuarioAtualService
    {
        Guid? UsuarioId { get; }


        string? Nome { get; }

        RoleUsuario? Role { get; }
        Guid? VendedorId { get; }
        string? Ip { get; }

        bool EhGerente { get; }
    }
}
