using Epinephrine.Models;
using Epinephrine.Interfaces;
using System.Collections.Concurrent;

namespace Epinephrine.Resolvers;

internal class OpenGenericResolver
{
    private readonly ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> _servicesDictionary;
    private readonly IResolver _resolver;
    private readonly IServiceResolver _serviceResolver;

    internal OpenGenericResolver(ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> servicesDictionary, IResolver resolver, IServiceResolver serviceResolver)
    {
        _servicesDictionary = servicesDictionary;
        _resolver = resolver;
        _serviceResolver = serviceResolver;
    }

    internal IEnumerable<object> Resolve(Type targetType)
    {
        var targetTypeGenericDefinition = targetType.GetGenericTypeDefinition();
        if (!_servicesDictionary.TryGetValue(targetTypeGenericDefinition, out var registeredServiceInstanceInfos))
            throw new NotSupportedException();

        foreach (var (instanceType, instanceInfo) in registeredServiceInstanceInfos)
        {
            var closeGenericInstanceType = instanceType.MakeGenericType(targetType.GetGenericArguments());
            if (!closeGenericInstanceType.IsAssignableTo(targetType))
                throw new NotSupportedException();

            if (instanceInfo.InstanceFunc is not null)
            {
                yield return instanceInfo.InstanceFunc(_serviceResolver);
                yield break;
            }

            if (instanceType.IsGenericTypeDefinition)
            {
                yield return _resolver.ResolveWithConstructor(closeGenericInstanceType);
                yield break;
            }

            throw new NotSupportedException();
        }
    }
}