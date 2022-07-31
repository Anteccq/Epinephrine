using System.Collections.Concurrent;
using Epinephrine.Interfaces;
using Epinephrine.Models;
using Epinephrine.Resolvers;

namespace Epinephrine;

internal class ServiceResolver : IServiceResolver
{
    private readonly IResolver _resolver;

    public ServiceResolver(ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> servicesDictionary) : this(servicesDictionary, new ConcurrentDictionary<Type, object>())
    {
    }

    internal ServiceResolver(ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>> servicesDictionary, ConcurrentDictionary<Type, object> resolvedSingletonService)
    {
        _resolver = new InternalResolver(servicesDictionary, resolvedSingletonService, this);
    }

    internal ServiceResolver(IResolver resolver)
    {
        _resolver = resolver;
    }

    public T ResolveService<T>()
        => (T)_resolver.ResolveServices(typeof(T)).First();

    public object ResolveService(Type requireServiceType)
        => _resolver.ResolveServices(requireServiceType).First();

    public IEnumerable<T> ResolveServices<T>()
        => _resolver.ResolveServices(typeof(T)).Cast<T>();

    public IEnumerable<object> ResolveServices(Type requireServiceType)
        => _resolver.ResolveServices(requireServiceType);
}
