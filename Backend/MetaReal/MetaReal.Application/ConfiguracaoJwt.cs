namespace MetaReal.Application
{
    public class ConfiguracaoJwt
    {
        public const string SecaoConfig = "Jwt";

        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenMinutos { get; set; } = 15;
        public int RefreshTokenDias { get; set; } = 7;
    }
}
