using Epinephrine.Models;

namespace Epinephrine.Resolvers;

internal interface IResolver
{
    internal IEnumerable<object> ResolveServices(Type requireServiceType);

    internal object Resolve(Type implementType, ServiceInstanceInfo instanceInfo);

    internal object ResolveWithConstructor(Type implementType);
}