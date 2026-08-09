using MetaReal.Application.DTO;

namespace MetaReal.Application.Interfaces
{
    public interface IAutenticacaoService
    {
        Task<ResultadoLoginDTO> Login(LoginEntradaDTO request, string? ip);

        Task<ResultadoLoginDTO> RefreshToken(string refreshTokenPlano, string? ip);

        Task Logout(string refreshTokenPlano);
    }
}
