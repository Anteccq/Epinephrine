namespace Epinephrine.Extensions;

internal static class TypeExtensions
{
    internal static bool IsResolvableIEnumerableType(this Type self)
        => self.IsGenericType &&
           self.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
           self.GetGenericArguments().Length == 1;
}