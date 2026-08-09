using MetaReal.Application.DTO;

namespace MetaReal.Application.Interfaces
{
    public interface IAuditoriaService
    {

        Task Registrar(string acao, string entidade, string? idEntidade, string? detalhes = null);


        Task<AuditoriaPaginadaDTO> ObterPaginado(int pagina, int tamanhoPagina, string? entidade = null);
    }
}
