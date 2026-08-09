namespace MetaReal.Application
{
   
    public class ConflitoException : Exception
    {
        public ConflitoException(string message) : base(message) { }
    }

    public class AcessoNegadoException : Exception
    {
        public AcessoNegadoException(string message) : base(message) { }
    }
}
