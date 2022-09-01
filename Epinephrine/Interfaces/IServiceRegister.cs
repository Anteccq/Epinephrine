using Epinephrine.Models;

namespace Epinephrine.Interfaces;

public interface IServiceRegister
{
    IServiceRegister RegisterSingleton<TService, TImplement>();
    IServiceRegister RegisterTransient<TService, TImplement>();
    IServiceRegister RegisterSingleton<TService, TImplement>(Func<IServiceResolver, TImplement> implementFunc);
    IServiceRegister RegisterTransient<TService, TImplement>(Func<IServiceResolver, TImplement> implementFunc);
    IServiceRegister Register<TService, TImplement>(InstanceType instanceType);
    IServiceRegister Register<TService, TImplement>(InstanceType instanceType, Func<IServiceResolver, object> implementFunc);
    IServiceResolver CreateResolver();
}