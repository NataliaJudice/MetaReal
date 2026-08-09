namespace MetaReal.Application.DTO
{
    public class ResultadoLoginDTO
    {
        public SessaoDTO Sessao { get; set; } = new SessaoDTO();
        public string RefreshTokenPlano { get; set; } = string.Empty;
    }
}
