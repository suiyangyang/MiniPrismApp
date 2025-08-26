namespace MiniPrismKit.Utils
{
    public static class Guards
    {
        public static void ThrowIfAnyNull(params object[] parameters)
        {
            if (parameters.Any(item => item == null))
                throw new ArgumentNullException();
        }
    }
}
