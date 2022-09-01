using Epinephrine.Interfaces;

namespace Epinephrine.Models;

internal record ServiceInstanceInfo(InstanceType InstanceType, Func<IServiceResolver, object>? InstanceFunc);