namespace MetaReal.Application.DTO
{
    public class AuditoriaDTO
    {
        public Guid Id { get; set; }
        public string? UsuarioNome { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string Entidade { get; set; } = string.Empty;
        public string? IdEntidade { get; set; }
        public DateTime DataHora { get; set; }
        public string? Detalhes { get; set; }
        public string? Ip { get; set; }
    }
}
