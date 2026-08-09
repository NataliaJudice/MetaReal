using MetaReal.Application.DTO;

namespace MetaReal.Application.Interfaces
{
    public interface IVendasService
    {
        Task<RegistroVendaPaginadoDTO> ObterTodos( Guid? vendedorId,   DateTime? dataInicio,DateTime? dataFim, int pagina,  int tamanhoPagina);

        Task<RegistroVendaDTO?> ObterPorId(Guid id);

        Task<RegistroVendaDTO> Adicionar(RegistroVendaEntradaDTO request);

        Task<RegistroVendaDTO> Editar(Guid id, RegistroVendaEntradaDTO request);

        Task Deletar(Guid id);

        Task<DashboardResumoDTO> ObterResumoGeral(DateTime? dataInicio, DateTime? dataFim);


        Task<PerfilVendedorDTO> ObterPerfilVendedor(  Guid vendedorId, DateTime? dataInicio,  DateTime? dataFim, int pagina,  int tamanhoPagina);
    }
}
