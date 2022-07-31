namespace Epinephrine.Interfaces;

public interface IServiceResolver
{
    T ResolveService<T>();

    object ResolveService(Type requireServiceType);

    IEnumerable<T> ResolveServices<T>();

    IEnumerable<object> ResolveServices(Type requireServiceType);
}