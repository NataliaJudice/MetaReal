using System;

namespace MetaReal.Domain.Models
{
    public class RefreshToken
    {
        public Guid IdRefreshToken { get; set; }
        public Guid IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        public string TokenHash { get; set; }

        public DateTime CriadoEm { get; set; }
        public DateTime ExpiraEm { get; set; }
        public DateTime? RevogadoEm { get; set; }
        public string? CriadoPorIp { get; set; }

        public bool EstaAtivo => RevogadoEm == null && ExpiraEm > DateTime.UtcNow;
    }
}
