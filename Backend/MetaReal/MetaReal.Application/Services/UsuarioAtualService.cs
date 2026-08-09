using System.Security.Claims;

using MetaReal.Application.Interfaces;


using MetaReal.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace MetaReal.Application.Services;

public class UsuarioAtualService : IUsuarioAtualService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAtualService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UsuarioId
    {
        get
        {
            var valor = BuscarClaim(ClaimTypes.NameIdentifier);

            Guid id;
            if (Guid.TryParse(valor, out id))
            {
                return id;
            }

            return null;
        }
    }

    public string? Nome
    {
        get
        {

            return BuscarClaim(ClaimTypes.Name);
        }
    }
    public RoleUsuario? Role
    {
        get
        {
            var valor = BuscarClaim(ClaimTypes.Role);


            RoleUsuario role;

            if (Enum.TryParse(valor, out role))
            {
                return role;
            }

            return null;
        }
    }

    public Guid? VendedorId
    {
        get
        {
            var valor = BuscarClaim("vendedorId");


            Guid id;
            if (Guid.TryParse(valor, out id))
            {

                return id;
            }
            return null;
        }
    }

    public string? Ip
    {
        get
        {

            var contexto = _httpContextAccessor.HttpContext;

            if (contexto == null)
            {
                return null;
            }

            var endereco = contexto.Connection.RemoteIpAddress;
            if (endereco == null)
            {
                return null;
            }

            return endereco.ToString();
        }
    }

    public bool EhGerente
    {
        get
        {
            return Role == RoleUsuario.Gerente;
        }
    }

    private string? BuscarClaim(string tipoDaClaim)
    {
        var contexto = _httpContextAccessor.HttpContext;
        if (contexto?.User == null)
        {
            return null;
        }

        return contexto.User.FindFirst(tipoDaClaim)?.Value;
    }
}
