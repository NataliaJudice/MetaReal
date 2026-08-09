using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System.Security.Cryptography;
using System.Text;

using MetaReal.Application;
using MetaReal.Application.DTO;
using MetaReal.Application.Interfaces;
using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MetaReal.Application.Services
{
    public class AutenticacaoService : IAutenticacaoService
    {
        private readonly IMetaRealDbContext _context;
        private readonly ConfiguracaoJwt _jwtSettings;

        public AutenticacaoService(IMetaRealDbContext context, IOptions<ConfiguracaoJwt> jwtOptions)
        {
            _context = context;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<ResultadoLoginDTO> Login(LoginEntradaDTO request, string? ip)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            {
                throw new UnauthorizedAccessException("E-mail ou senha inválidos.");
            }

            var emailNormalizado = request.Email.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios
                .Include(u => u.Vendedor)
                .FirstOrDefaultAsync(u => u.Email == emailNormalizado);

            if (usuario == null || !usuario.Ativo || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
 
                throw new UnauthorizedAccessException("E-mail ou senha inválidos.");
            }
            return await GerarTokens(usuario, ip);
        }
        public async Task<ResultadoLoginDTO> RefreshToken(string refreshTokenPlano, string? ip)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenPlano))
            {
                throw new UnauthorizedAccessException("Sessão inválida. Faça login novamente.");
            }
            var hash = HashToken(refreshTokenPlano);
            var tokenAtual = await _context.RefreshTokens
                .Include(t => t.Usuario)
                .ThenInclude(u => u.Vendedor)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (tokenAtual == null || !tokenAtual.EstaAtivo ||   !tokenAtual.Usuario.Ativo)
            {
                throw new UnauthorizedAccessException("Sessão expirada. Faça login novamente.");
            }

            tokenAtual.RevogadoEm = DateTime.UtcNow;
            var resultado = await GerarTokens(tokenAtual.Usuario, ip);

            return resultado;
        }


        public async Task Logout(string refreshTokenPlano)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenPlano))
            {
                return;
            }

            var hash = HashToken(refreshTokenPlano);
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (token != null && token.RevogadoEm == null)
            {
                token.RevogadoEm = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }


        private async Task<ResultadoLoginDTO> GerarTokens(Usuario usuario, string? ip)
        {
            var accessToken = GerarAccessToken(usuario);
            var refreshTokenPlano = GerarRefreshTokenOpaco();
            var novoRefreshToken = new RefreshToken();

            novoRefreshToken.IdRefreshToken = Guid.NewGuid();

            novoRefreshToken.IdUsuario = usuario.IdUsuario;
            novoRefreshToken.TokenHash = HashToken(refreshTokenPlano);
            novoRefreshToken.CriadoEm = DateTime.UtcNow;
            novoRefreshToken.ExpiraEm = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDias);
            novoRefreshToken.CriadoPorIp = ip;

            _context.RefreshTokens.Add(novoRefreshToken);
            await _context.SaveChangesAsync();

            var usuarioDto = new UsuarioDTO();
            usuarioDto.Id = usuario.IdUsuario;
            usuarioDto.Nome = usuario.Nome;
            usuarioDto.Email = usuario.Email;
            usuarioDto.Role = usuario.Role.ToString();
            usuarioDto.VendedorId = usuario.IdVendedor;

            var sessao = new SessaoDTO();
            sessao.AccessToken = accessToken;

            sessao.ExpiraEmSegundos = _jwtSettings.AccessTokenMinutos * 60;
            sessao.Usuario = usuarioDto;

            var resultado = new ResultadoLoginDTO();
            resultado.Sessao = sessao;
            resultado.RefreshTokenPlano = refreshTokenPlano;

            return resultado;
        }
        private string GerarAccessToken(Usuario usuario)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, usuario.Nome));
            claims.Add(new Claim(ClaimTypes.Email, usuario.Email));

            claims.Add(new Claim(ClaimTypes.Role, usuario.Role.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            if (usuario.IdVendedor.HasValue)
            {
                claims.Add(new Claim("vendedorId", usuario.IdVendedor.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutos),
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GerarRefreshTokenOpaco() {

            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
           

        private static string HashToken(string token)
        {


            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        }
            

    }
}
