using MiniPrismKit.MVVM;

namespace MiniPrismKit.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsInheritsFrom(this Type @this, Type interfaceType)
        {
            return (
                    from @interface in @this.GetInterfaces()
                    where @interface.Name == interfaceType.Name
                    select @interface
                )
                .FirstOrDefault() == null;
        }
        public static IGenericInterface AsGenericInterface(this object @this, Type type)
        {
            var interfaceType = (
                    from @interface in @this.GetType().GetInterfaces()
                    where @interface.IsGenericType
                    let definition = @interface.GetGenericTypeDefinition()
                    where definition == type
                    select @interface
                )
                .SingleOrDefault();

            return interfaceType != null
                ? new GenericInterfaceImpl(@this, interfaceType)
                : null;
        }
    }
}
