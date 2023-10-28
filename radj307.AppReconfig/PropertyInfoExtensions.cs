using System.Reflection;
using System.Runtime.CompilerServices;

namespace AppConfig
{
    internal static class PropertyInfoExtensions
    {
        internal static bool IsStatic(this PropertyInfo propertyInfo)
            => propertyInfo.GetMethod?.IsStatic ?? propertyInfo.SetMethod!.IsStatic;
        internal static bool IsAutoImplemented(this PropertyInfo propertyInfo)
            => (propertyInfo.GetMethod != null && propertyInfo.GetMethod.GetCustomAttribute<CompilerGeneratedAttribute>(true) != null)
            || (propertyInfo.SetMethod != null && propertyInfo.SetMethod.GetCustomAttribute<CompilerGeneratedAttribute>(true) != null);

    }
}