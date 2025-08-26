namespace MiniPrismKit.MVVM
{
    public interface IGenericInterface
    {
        Type Type { get; }

        Type[] GenericArguments { get; }

        TDelegate GetMethod<TDelegate>(string methodName, params Type[] argTypes);
    }
}
