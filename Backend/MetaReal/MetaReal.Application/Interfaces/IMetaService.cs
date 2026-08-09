using MetaReal.Application.DTO;

namespace MetaReal.Application.Interfaces
{
    public interface IMetaService
    {
        Task<MetaProgressoDTO> DefinirMeta(MetaEntradaDTO request);


        Task<List<MetaProgressoDTO>> DefinirMetaParaTodos(MetaLoteEntradaDTO request);

        Task<MetaProgressoDTO> ObterProgresso(Guid vendedorId, int? mes, int? ano);

        Task<List<MetaProgressoDTO>> ObterProgressoGeral(int? mes, int? ano);


        Task<List<MetaProgressoDTO>> ObterHistorico(Guid vendedorId, int meses);

 

        Task VerificarConclusao(Guid vendedorId, DateTime data);
    }
}
