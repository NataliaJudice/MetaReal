using System;
using System.Collections.Generic;

namespace MetaReal.Domain.Models
{
    public class Usuario
    {
        public Guid IdUsuario { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
        public RoleUsuario Role { get; set; }

        public Guid? IdVendedor { get; set; }
        public Vendedor? Vendedor { get; set; }

        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
