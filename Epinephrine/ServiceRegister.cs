using System.Collections.Concurrent;
using Epinephrine.Interfaces;
using Epinephrine.Models;

namespace Epinephrine;

public class ServiceRegister : IServiceRegister
{
    private readonly ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> _registeredServices;

    public ServiceRegister() : this(new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>())
    {

    }

    internal ServiceRegister(ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> registeredServices)
    {
        _registeredServices = registeredServices;
    }

    public IServiceRegister RegisterSingleton<TService, TImplement>()
        => Register<TService, TImplement>(InstanceType.Singleton);

    public IServiceRegister RegisterTransient<TService, TImplement>()
        => Register<TService, TImplement>(InstanceType.Transient);

    public IServiceRegister RegisterSingleton<TService, TImplement>(Func<IServiceResolver, TImplement> implementFunc)
        => Register<TService, TImplement>(InstanceType.Singleton, x => implementFunc(x)!);

    public IServiceRegister RegisterTransient<TService, TImplement>(Func<IServiceResolver, TImplement> implementFunc)
        => Register<TService, TImplement>(InstanceType.Transient, x => implementFunc(x)!);

    public IServiceRegister Register<TService, TImplement>(InstanceType instanceType)
    {
        var implementType = typeof(TImplement);
        var serviceType = typeof(TService);

        if (implementType.GetInterface(serviceType.Name) is null)
            throw new ArgumentException($"{implementType.Name} must be implement {serviceType.Name}");

        RegisterService(serviceType, implementType, instanceType, null);

        return this;
    }

    public IServiceRegister Register<TService, TImplement>(InstanceType instanceType, Func<IServiceResolver, object> implementFunc)
    {
        var implementType = typeof(TImplement);
        var serviceType = typeof(TService);

        if (implementType.GetInterface(serviceType.Name) is null)
            throw new ArgumentException($"{implementType.Name} must be implement {serviceType.Name}");

        RegisterService(serviceType, implementType, instanceType, implementFunc);

        return this;
    }

    private void RegisterService(Type serviceType, Type implementType, InstanceType instanceType, Func<IServiceResolver, object>? implementFunc)
    {
        if (_registeredServices.ContainsKey(serviceType))
        {
            var alreadyRegisteredService = _registeredServices.GetValueOrDefault(serviceType);
            if (alreadyRegisteredService!.ContainsKey(implementType))
                throw new ArgumentException($"TService:{serviceType.Name} TImplement:{implementType.Name} already registered");

            alreadyRegisteredService.Add(implementType, new ServiceInstanceInfo(instanceType, implementFunc));
        }
        else
        {
            var serviceInstances = new Dictionary<Type, ServiceInstanceInfo>
            {
                {implementType, new ServiceInstanceInfo(instanceType, implementFunc)}
            };
            _registeredServices.TryAdd(serviceType, serviceInstances);
        }
    }

    public IServiceResolver CreateResolver()
        => new ServiceResolver(_registeredServices);
}