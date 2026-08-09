using MetaReal.Application;
using MetaReal.Application.DTO;
using MetaReal.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MetaReal.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AutenticacaoController : ControllerBase
{
    private const string CookieRefresh = "refreshToken";

    private readonly IAutenticacaoService _authService;
    private readonly ConfiguracaoJwt _jwtSettings;

    public AutenticacaoController(IAutenticacaoService authService, IOptions<ConfiguracaoJwt> jwtOptions)
    {
        _authService = authService;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginEntradaDTO request)
    {

        try
        {
            var resultado = await _authService.Login(request, ObterIp());

            DefinirCookieRefresh(resultado.RefreshTokenPlano);


            return Ok(new { success = true, data = resultado.Sessao });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshTokenAtual = Request.Cookies[CookieRefresh];
        if (string.IsNullOrEmpty(refreshTokenAtual))
        {
            return Unauthorized(new { success = false, error = "Sessão inválida. Faça login novamente." });
        }

        try
        {
            var resultado = await _authService.RefreshToken(refreshTokenAtual, ObterIp());

            DefinirCookieRefresh(resultado.RefreshTokenPlano);
            return Ok(new { success = true, data = resultado.Sessao });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, error = ex.Message });
        }
    }



    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshTokenAtual = Request.Cookies[CookieRefresh];
        if (!string.IsNullOrEmpty(refreshTokenAtual))
        {
            await _authService.Logout(refreshTokenAtual);
        }

        Response.Cookies.Delete(CookieRefresh, new CookieOptions { Path = "/api/auth" });
        return Ok(new { success = true });
    }

    private void DefinirCookieRefresh(string refreshTokenPlano)
    {

        Response.Cookies.Append(CookieRefresh, refreshTokenPlano, new CookieOptions
        {

            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth", // fica só nas rotas de auth, não precisa ir junto em toda requisição

            Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenDias)
        });
    }

    private string? ObterIp() {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
