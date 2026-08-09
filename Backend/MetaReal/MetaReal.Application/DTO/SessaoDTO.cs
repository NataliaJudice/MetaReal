namespace MetaReal.Application.DTO
{
    public class SessaoDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiraEmSegundos { get; set; }
        public UsuarioDTO Usuario { get; set; } = new();
    }
}
