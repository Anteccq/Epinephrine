using Epinephrine.Interfaces;

namespace Epinephrine.Models;

public record ServiceInstanceInfo(InstanceType InstanceType, Func<IServiceResolver, object>? InstanceFunc);