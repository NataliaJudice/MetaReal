using System;

namespace MetaReal.Domain.Models
{
    public class AuditLog
    {
        public Guid IdAuditLog { get; set; }

        public Guid? IdUsuario { get; set; }
        public string? UsuarioNome { get; set; }

        public string Acao { get; set; }
        public string Entidade { get; set; }
        public string? IdEntidade { get; set; }
        public DateTime DataHora { get; set; }
        public string? Detalhes { get; set; }
        public string? Ip { get; set; }
    }
}
