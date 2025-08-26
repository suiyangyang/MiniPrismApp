namespace MiniPrismKit.Services.Exceptions
{
    public class AppException : Exception
    {
        public AppException() { }

        public AppException(string msg) : base(msg) { }
    }
}
