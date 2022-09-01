using Epinephrine.Models;
using System.Reflection;

namespace Epinephrine.Resolvers;

internal class EnumerableResolver
{
    private readonly IResolver _resolver;

    private static readonly MethodInfo CastMethodInfo = typeof(Enumerable).GetMethod("Cast", BindingFlags.Public | BindingFlags.Static)!;

    internal EnumerableResolver(IResolver resolver)
    {
        _resolver = resolver;
    }

    public object Resolve(Type requiredType, IDictionary<Type, ServiceInstanceInfo> registeredServiceInstanceInfos)
    {
        var targetArgumentType = requiredType.GetGenericArguments().Single();
        var methodInfo = CastMethodInfo.MakeGenericMethod(targetArgumentType);

        var instances =
            registeredServiceInstanceInfos
                .Select(infoPair =>
                {
                    if(!targetArgumentType.IsAssignableFrom(infoPair.Key))
                        throw new NotSupportedException();

                    return _resolver.Resolve(infoPair.Key, infoPair.Value);
                });

        var result = methodInfo.Invoke(null, new object[] { instances });
        return result!;
    }
}