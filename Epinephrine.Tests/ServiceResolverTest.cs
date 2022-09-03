using Epinephrine.Models;
using System.Collections.Concurrent;
using FluentAssertions;

namespace Epinephrine.Tests;

public class ServiceResolverTest
{
    [Fact]
    public void ResolveNoParameterConstructorTypeTest()
    {
        var implementInfoDictionary = new Dictionary<Type, ServiceInstanceInfo>
        {
            { typeof(ConstructorWithNoParameterTestClass), new ServiceInstanceInfo(InstanceType.Transient, null) }
        };
        var serviceDictionary = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        serviceDictionary.TryAdd(typeof(ITestInterface), implementInfoDictionary);

        var resolver = new ServiceResolver(serviceDictionary);
        var instance = resolver.ResolveService<ITestInterface>();
        instance.Should().BeOfType<ConstructorWithNoParameterTestClass>();
    }

    [Fact]
    public void ResolveWithImplementFuncTest()
    {
        const string expectedKey = "ResolveWithImplementFuncTest-key";

        var implementInfoDictionary = new Dictionary<Type, ServiceInstanceInfo>
        {
            { typeof(ConstructorWithParameterNoDependencyOtherServiceTestClass), new ServiceInstanceInfo(InstanceType.Transient, x => new ConstructorWithParameterNoDependencyOtherServiceTestClass(expectedKey)) }
        };
        var serviceDictionary = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        serviceDictionary.TryAdd(typeof(ITestInterface), implementInfoDictionary);

        var resolver = new ServiceResolver(serviceDictionary);
        var instance = resolver.ResolveService<ITestInterface>();
        instance.Should().BeOfType<ConstructorWithParameterNoDependencyOtherServiceTestClass>();
        instance.Key.Should().Be(expectedKey);
    }

    [Fact]
    public void ResolveConstructorWithParameterType_HasDependencyRegisteredServiceTest()
    {
        const string expectedKey = "ResolveConstructorWithParameterType_HasDependencyRegisteredServiceTest-key";

        var serviceDictionary = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();

        serviceDictionary.TryAdd(typeof(ITestInterface), new Dictionary<Type, ServiceInstanceInfo>
        {
            {
                typeof(ConstructorWithParameterNoDependencyOtherServiceTestClass),
                new ServiceInstanceInfo(InstanceType.Transient, _ => new ConstructorWithParameterNoDependencyOtherServiceTestClass(expectedKey))
            }
        });

        serviceDictionary.TryAdd(typeof(IDependentTest),
            new Dictionary<Type, ServiceInstanceInfo>
            {
                {
                    typeof(ConstructorWithParameterDependsOtherServiceTestClass),
                    new ServiceInstanceInfo(InstanceType.Transient, null)
                }
            });

        var resolver = new ServiceResolver(serviceDictionary);
        var instance = resolver.ResolveService<IDependentTest>();
        instance.Should().BeOfType<ConstructorWithParameterDependsOtherServiceTestClass>();
        instance.InnerService.Should().BeOfType<ConstructorWithParameterNoDependencyOtherServiceTestClass>();
        instance.InnerService.Key.Should().Be(expectedKey);
    }

    [Fact]
    public void ResolveConstructorWithEnumerableServiceParameterTypeTest()
    {
        const string expectedKey = "ResolveConstructorWithParameterType_HasDependencyRegisteredServiceTest-key";

        var serviceDictionary = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();

        serviceDictionary.TryAdd(typeof(ITestInterface), new Dictionary<Type, ServiceInstanceInfo>
        {
            {
                typeof(ConstructorWithParameterNoDependencyOtherServiceTestClass),
                new ServiceInstanceInfo(InstanceType.Transient, _ => new ConstructorWithParameterNoDependencyOtherServiceTestClass(expectedKey))
            },
            {
                typeof(ConstructorWithNoParameterTestClass),
                new ServiceInstanceInfo(InstanceType.Transient, null)
            }
        });

        serviceDictionary.TryAdd(typeof(IEnumerableDependentsTest),
            new Dictionary<Type, ServiceInstanceInfo>
            {
                {
                    typeof(ConstructorWithEnumerableServiceParameterTestClass),
                    new ServiceInstanceInfo(InstanceType.Transient, null)
                }
            });

        var resolver = new ServiceResolver(serviceDictionary);
        var instance = resolver.ResolveService<IEnumerableDependentsTest>();
        instance.Should().BeOfType<ConstructorWithEnumerableServiceParameterTestClass>();
        instance.InnerServices.Should().HaveCount(2);
        instance.InnerServices.Should().Contain(x => x.GetType() == typeof(ConstructorWithNoParameterTestClass));
        instance.InnerServices.Should().Contain(x => x.GetType() == typeof(ConstructorWithParameterNoDependencyOtherServiceTestClass));

        var requireEnumerableInstance = resolver.ResolveService<IEnumerable<ITestInterface>>().ToArray();
        requireEnumerableInstance.Should().HaveCount(2);
        requireEnumerableInstance.Should().Contain(x => x.GetType() == typeof(ConstructorWithNoParameterTestClass));
        requireEnumerableInstance.Should().Contain(x => x.GetType() == typeof(ConstructorWithParameterNoDependencyOtherServiceTestClass));
    }

    [Fact]
    public void ResolveOpenGenericTypeTest()
    {
        var serviceDictionary = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        serviceDictionary.TryAdd(typeof(ITestInterface), new Dictionary<Type, ServiceInstanceInfo>
        {
            {
                typeof(ConstructorWithGenericType),
                new ServiceInstanceInfo(InstanceType.Transient, null)
            }
        });
        serviceDictionary.TryAdd(typeof(IGenericType<>), new Dictionary<Type, ServiceInstanceInfo>
        {
            {
                typeof(GenericTypeTestClass<>),
                new ServiceInstanceInfo(InstanceType.Transient, null)
            }
        });

        var resolver = new ServiceResolver(serviceDictionary);
        var instance = resolver.ResolveService<ITestInterface>();
        instance.Should().BeOfType<ConstructorWithGenericType>();

        var resolvedCloseGenericTypeInstance = resolver.ResolveService<IGenericType<ConstructorWithNoParameterTestClass>>();
        resolvedCloseGenericTypeInstance.Should().BeOfType<GenericTypeTestClass<ConstructorWithNoParameterTestClass>>();
    }

    [Fact]
    public void ResolveSingletonInstanceTypeTest()
    {
        var implementInfoDictionary = new Dictionary<Type, ServiceInstanceInfo>
        {
            { typeof(ConstructorWithNoParameterTestClass), new ServiceInstanceInfo(InstanceType.Singleton, null) }
        };
        var serviceDictionary = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        serviceDictionary.TryAdd(typeof(ITestInterface), implementInfoDictionary);

        var resolver = new ServiceResolver(serviceDictionary);
        var instance = resolver.ResolveService<ITestInterface>();
        var instance2 = resolver.ResolveService<ITestInterface>();
        instance.Should().Be(instance2);
    }

    [Fact]
    public void ResolveTransientInstanceTypeTest()
    {
        var implementInfoDictionary = new Dictionary<Type, ServiceInstanceInfo>
        {
            { typeof(ConstructorWithNoParameterTestClass), new ServiceInstanceInfo(InstanceType.Transient, null) }
        };
        var serviceDictionary = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        serviceDictionary.TryAdd(typeof(ITestInterface), implementInfoDictionary);

        var resolver = new ServiceResolver(serviceDictionary);
        var instance = resolver.ResolveService<ITestInterface>();
        var instance2 = resolver.ResolveService<ITestInterface>();
        instance.Should().NotBe(instance2);
    }

    public interface ITestInterface
    {
        string Key { get; }
    }

    public class ConstructorWithNoParameterTestClass : ITestInterface
    {
        public ConstructorWithNoParameterTestClass()
        {
            Key = "test-Key";
        }

        public string Key { get; }
    }

    public class ConstructorWithParameterNoDependencyOtherServiceTestClass : ITestInterface
    {
        public ConstructorWithParameterNoDependencyOtherServiceTestClass(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }

    public interface IDependentTest
    {
        ITestInterface InnerService { get; }
    }

    public class ConstructorWithParameterDependsOtherServiceTestClass : IDependentTest
    {
        public ConstructorWithParameterDependsOtherServiceTestClass(ITestInterface dependService)
        {
            InnerService = dependService;
        }

        public ITestInterface InnerService { get; }
    }

    public interface IEnumerableDependentsTest
    {
        IEnumerable<ITestInterface> InnerServices { get; }
    }

    public class ConstructorWithEnumerableServiceParameterTestClass : IEnumerableDependentsTest
    {
        public ConstructorWithEnumerableServiceParameterTestClass(IEnumerable<ITestInterface> dependsService)
        {
            InnerServices = dependsService;
        }

        public IEnumerable<ITestInterface> InnerServices { get; }
    }

    public interface IGenericType<T>
    {
        string GetTypeParameter();
    }

    public class GenericTypeTestClass<T> : IGenericType<T>
    {
        public GenericTypeTestClass()
        {
        }

        public string GetTypeParameter()
            => typeof(T).Name;
    }

    public class ConstructorWithGenericType : ITestInterface
    {
        public ConstructorWithGenericType(IGenericType<ConstructorWithGenericType> content)
        {
            Key = content.GetTypeParameter();
        }

        public string Key { get; }
    }
}