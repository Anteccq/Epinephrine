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

    public IServiceRegister RegisterSingleton<TService, TImplement>(Func<IServiceResolver, TImplement>? implementFunc = null)
        => Register<TService, TImplement>(InstanceType.Singleton, implementFunc as Func<IServiceResolver, object>);

    public IServiceRegister RegisterTransient<TService, TImplement>(Func<IServiceResolver, TImplement>? implementFunc = null)
        => Register<TService, TImplement>(InstanceType.Transient, implementFunc as Func<IServiceResolver, object>);

    public IServiceRegister RegisterSingleton(Type serviceType, Type implementType, Func<IServiceResolver, object>? implementFunc = null)
        => Register(serviceType, implementType, InstanceType.Singleton, implementFunc);

    public IServiceRegister RegisterTransient(Type serviceType, Type implementType, Func<IServiceResolver, object>? implementFunc = null)
        => Register(serviceType, implementType, InstanceType.Transient, implementFunc);

    public IServiceRegister Register<TService, TImplement>(InstanceType instanceType, Func<IServiceResolver, object>? implementFunc = null)
        => Register(typeof(TService), typeof(TImplement), instanceType, implementFunc);

    public IServiceRegister Register(Type serviceType, Type implementType, InstanceType instanceType, Func<IServiceResolver, object>? implementFunc = null)
    {
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