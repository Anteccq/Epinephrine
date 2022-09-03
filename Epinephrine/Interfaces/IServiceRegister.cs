using Epinephrine.Models;

namespace Epinephrine.Interfaces;

public interface IServiceRegister
{
    IServiceRegister RegisterSingleton<TService, TImplement>(Func<IServiceResolver, TImplement>? implementFunc = null);
    IServiceRegister RegisterTransient<TService, TImplement>(Func<IServiceResolver, TImplement>? implementFunc = null);
    IServiceRegister Register<TService, TImplement>(InstanceType instanceType, Func<IServiceResolver, object>? implementFunc = null);
    IServiceRegister Register(Type serviceType, Type implementType, InstanceType instanceType, Func<IServiceResolver, object>? implementFunc = null);
    IServiceResolver CreateResolver();
}