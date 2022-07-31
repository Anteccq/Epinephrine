using System.Collections;
using System.Collections.Concurrent;
using Epinephrine.Interfaces;
using Epinephrine.Models;

namespace Epinephrine;

internal class ServiceResolver : IServiceResolver
{
    private readonly IReadOnlyDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> _servicesDictionary;

    private readonly ConcurrentDictionary<Type, object> _resolvedSingletonService;

    public ServiceResolver(ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> servicesDictionary) : this(servicesDictionary, new ConcurrentDictionary<Type, object>())
    {
    }

    internal ServiceResolver(ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> servicesDictionary, ConcurrentDictionary<Type, object> resolvedSingletonService)
    {
        _servicesDictionary = servicesDictionary;
        _resolvedSingletonService = resolvedSingletonService;
    }

    public T ResolveService<T>()
        => Resolve<T>().First();

    public object ResolveService(Type requireServiceType)
        => Resolve(requireServiceType).First();

    private IEnumerable<T> Resolve<T>()
    {
        var requireServiceType = typeof(T);
        _servicesDictionary.TryGetValue(requireServiceType, out var infoDictionary);

        foreach (var (implementType, instanceInfo) in infoDictionary!)
        {
            if (instanceInfo.InstanceType == InstanceType.Singleton && _resolvedSingletonService.TryGetValue(implementType, out var instance))
                yield return (T) instance;

            if (implementType == requireServiceType)
                yield return ResolveSpecifiedType<T>(infoDictionary[implementType]);

            if (instanceInfo.InstanceFunc is null)
                yield return ResolveWithNoImplementFunc<T>(implementType, instanceInfo.InstanceType);

            yield return ResolveWithImplementFunc<T>(implementType, instanceInfo);
        }
    }

    private IEnumerable<object> Resolve(Type requireServiceType)
    {
        _servicesDictionary.TryGetValue(requireServiceType, out var infoDictionary);

        foreach (var (implementType, instanceInfo) in infoDictionary!)
        {
            if (instanceInfo.InstanceType == InstanceType.Singleton && _resolvedSingletonService.TryGetValue(implementType, out var instance))
                yield return instance;

            if (implementType == requireServiceType)
                yield return ResolveSpecifiedType(requireServiceType, infoDictionary[implementType]);

            if (instanceInfo.InstanceFunc is null)
                yield return ResolveWithNoImplementFunc(requireServiceType, implementType, instanceInfo.InstanceType);

            yield return ResolveWithImplementFunc(requireServiceType, implementType, instanceInfo);
        }
    }

    private T ResolveSpecifiedType<T>(ServiceInstanceInfo info)
    {
        throw new NotImplementedException();
    }

    private object ResolveSpecifiedType(Type requiredType, ServiceInstanceInfo info)
    {
        throw new NotImplementedException();
    }

    private T ResolveWithNoImplementFunc<T>(Type implementType, InstanceType instanceType)
    {
        throw new NotImplementedException();
    }

    private object ResolveWithNoImplementFunc(Type requiredType, Type implementType, InstanceType instanceType)
    {
        throw new NotImplementedException();
    }

    private T ResolveWithImplementFunc<T>(Type implementType, ServiceInstanceInfo instanceInfo)
    {
        var instance = instanceInfo.InstanceFunc!(this);
        _resolvedSingletonService.TryAdd(implementType, instance);
        return (T)instance;
    }

    private object ResolveWithImplementFunc(Type requiredType, Type implementType, ServiceInstanceInfo instanceInfo)
    {
        var instance = instanceInfo.InstanceFunc!(this);
        _resolvedSingletonService.TryAdd(implementType, instance);
        return instance;
    }

    private T ResolveWithConstructor<T>(Type implementType)
    {
        var constructorInfos = implementType.GetConstructors();
        var targetConstructorArguments = constructorInfos.Select(x => x.GetParameters()).MaxBy(x => x.Length);

        if (targetConstructorArguments is null)
            throw new NotSupportedException();

        if (targetConstructorArguments.Length != 0)
        {
            var arguments = targetConstructorArguments.Select(x =>
            {
                if (x.ParameterType.GetInterface(nameof(IEnumerable)) is null)
                    return ResolveService(x.ParameterType);

                if (!x.ParameterType.IsConstructedGenericType)
                    throw new NotSupportedException();

                return Resolve(x.ParameterType);
            }).ToArray();
            
            var instance = Activator.CreateInstance(implementType, arguments);
            
            if (instance is not T result)
                throw new UriFormatException();

            return result;
        }
        else
        {
            var instance = Activator.CreateInstance(implementType);
            if (instance is not T result)
                throw new UriFormatException();
            
            return result;
        }
    }
}