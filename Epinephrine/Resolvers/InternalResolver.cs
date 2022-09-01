using Epinephrine.Models;
using System.Collections.Concurrent;
using Epinephrine.Interfaces;
using Epinephrine.Extensions;

namespace Epinephrine.Resolvers;

internal class InternalResolver : IResolver
{
    private readonly IReadOnlyDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> _servicesDictionary;
    private readonly ConcurrentDictionary<Type, object> _resolvedSingletonService;
    private readonly IServiceResolver _resolver;
    private readonly EnumerableResolver _enumerableResolver;
    private readonly OpenGenericResolver _openGenericResolver;

    internal InternalResolver(ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> servicesDictionary, ConcurrentDictionary<Type, object> resolvedSingletonService, IServiceResolver resolver)
    {
        _servicesDictionary = servicesDictionary;
        _resolvedSingletonService = resolvedSingletonService;
        _resolver = resolver;
        _enumerableResolver = new EnumerableResolver(this);
        _openGenericResolver = new OpenGenericResolver(servicesDictionary, this, resolver);
    }

    IEnumerable<object> IResolver.ResolveServices(Type requireServiceType)
        => ResolveServices(requireServiceType);

    private IEnumerable<object> ResolveServices(Type requireServiceType)
    {
        if (requireServiceType.IsResolvableIEnumerableType())
        {
            yield return _enumerableResolver.Resolve(requireServiceType, GetEnumerableInstanceInfosDictionary(requireServiceType));
            yield break;
        }
        else if (requireServiceType.IsGenericType)
        {
            foreach (var instance in _openGenericResolver.Resolve(requireServiceType))
                yield return instance;
            yield break;
        }

        _servicesDictionary.TryGetValue(requireServiceType, out var infoDictionary);

        foreach (var (implementType, instanceInfo) in infoDictionary!)
        {
            if (!implementType.IsAssignableTo(requireServiceType))
                throw new InvalidCastException();

            yield return Resolve(implementType, instanceInfo);
        }
    }

    object IResolver.Resolve(Type implementType, ServiceInstanceInfo instanceInfo)
        => Resolve(implementType, instanceInfo);

    private object Resolve(Type implementType, ServiceInstanceInfo instanceInfo)
    {
        if (instanceInfo.InstanceType == InstanceType.Singleton && _resolvedSingletonService.TryGetValue(implementType, out var instance))
            return instance;

        if (instanceInfo.InstanceFunc is not null)
            return ResolveWithImplementFunc(implementType, instanceInfo);

        return ResolveWithoutImplementFunc(implementType, instanceInfo.InstanceType);
    }

    private object ResolveWithoutImplementFunc(Type implementType, InstanceType instanceType)
    {
        var instance = ResolveWithConstructor(implementType);
        if (instanceType == InstanceType.Singleton)
            _resolvedSingletonService.TryAdd(implementType, instance);

        return instance;
    }

    private object ResolveWithImplementFunc(Type implementType, ServiceInstanceInfo instanceInfo)
    {
        var instance = instanceInfo.InstanceFunc!(_resolver);
        if (instanceInfo.InstanceType == InstanceType.Singleton)
            _resolvedSingletonService.TryAdd(implementType, instance);
        return instance;
    }

    object IResolver.ResolveWithConstructor(Type implementType)
        => ResolveWithConstructor(implementType);

    private object ResolveWithConstructor(Type implementType)
    {
        var constructorInfos = implementType.GetConstructors();
        var targetConstructorArguments = constructorInfos.Select(x => x.GetParameters()).MaxBy(x => x.Length);

        if (targetConstructorArguments is null)
            throw new NotSupportedException();

        if (targetConstructorArguments.Length == 0)
            return Activator.CreateInstance(implementType)!;

        var arguments =
            targetConstructorArguments
                .Select(x => ResolveWithConstructorCore(x.ParameterType))
                .ToArray();

        return Activator.CreateInstance(implementType, arguments)!;
    }

    private object ResolveWithConstructorCore(Type implementType)
    {
        if (implementType.IsResolvableIEnumerableType())
        {
            return _enumerableResolver.Resolve(implementType, GetEnumerableInstanceInfosDictionary(implementType));
        }
        
        if (implementType.IsGenericType)
        {
            return _openGenericResolver.Resolve(implementType).First();
        }

        return _resolver.ResolveService(implementType);
    }

    private IDictionary<Type, ServiceInstanceInfo> GetEnumerableInstanceInfosDictionary(Type type)
    {
        var parameterType = type.GetGenericArguments().Single();
        if (_servicesDictionary.TryGetValue(parameterType, out var instanceInfoDictionary))
            return instanceInfoDictionary;

        throw new NotSupportedException();
    }
}