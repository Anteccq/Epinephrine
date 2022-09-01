using System.Collections.Concurrent;
using Epinephrine.Interfaces;
using Epinephrine.Models;
using FluentAssertions;

namespace Epinephrine.Tests;

public class ServiceRegisterTest
{
    [Fact]
    public void RegisterSingleton_WithoutImplementFunc_Test()
    {
        var registeredServices = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        var serviceRegister = new ServiceRegister(registeredServices);
        var resultRegister = serviceRegister.RegisterSingleton<ITestService, TestImplementService>();

        resultRegister.Should().Be(serviceRegister);

        registeredServices.Should().HaveCount(1);
        registeredServices.TryGetValue(typeof(ITestService), out var instanceInfoDictionary).Should().BeTrue();
        instanceInfoDictionary.Should().HaveCount(1);
        instanceInfoDictionary!.TryGetValue(typeof(TestImplementService), out var info).Should().BeTrue();
        info!.InstanceType.Should().Be(InstanceType.Singleton);
        info.InstanceFunc.Should().BeNull();
    }

    [Fact]
    public void RegisterTransient_WithoutImplementFunc_Test()
    {
        var registeredServices = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        var serviceRegister = new ServiceRegister(registeredServices);
        var resultRegister = serviceRegister.RegisterTransient<ITestService, TestImplementService>();

        resultRegister.Should().Be(serviceRegister);

        registeredServices.Should().HaveCount(1);
        registeredServices.TryGetValue(typeof(ITestService), out var instanceInfoDictionary).Should().BeTrue();
        instanceInfoDictionary.Should().HaveCount(1);
        instanceInfoDictionary!.TryGetValue(typeof(TestImplementService), out var info).Should().BeTrue();
        info!.InstanceType.Should().Be(InstanceType.Transient);
        info.InstanceFunc.Should().BeNull();
    }

    [Fact]
    public void RegisterSingleton_WithImplementFunc_Test()
    {
        const string expectedKey = "key";

        var registeredServices = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        var serviceRegister = new ServiceRegister(registeredServices);
        var resultRegister = serviceRegister.RegisterSingleton<ITestService, TestImplementServiceWithConstructor>(_ =>
            new TestImplementServiceWithConstructor(expectedKey));

        resultRegister.Should().Be(serviceRegister);

        registeredServices.Should().HaveCount(1);
        registeredServices.TryGetValue(typeof(ITestService), out var instanceInfoDictionary).Should().BeTrue();
        instanceInfoDictionary.Should().HaveCount(1);
        instanceInfoDictionary!.TryGetValue(typeof(TestImplementServiceWithConstructor), out var info).Should().BeTrue();
        info!.InstanceType.Should().Be(InstanceType.Singleton);
        ((TestImplementServiceWithConstructor)info.InstanceFunc!(new TestServiceResolver())).Key.Should().Be(expectedKey);
    }

    [Fact]
    public void RegisterTransient_WithImplementFunc_Test()
    {
        const string expectedKey = "key";

        var registeredServices = new ConcurrentDictionary<Type, IDictionary<Type, ServiceInstanceInfo>>();
        var serviceRegister = new ServiceRegister(registeredServices);
        var resultRegister = serviceRegister.RegisterTransient<ITestService, TestImplementServiceWithConstructor>(_ =>
            new TestImplementServiceWithConstructor(expectedKey));

        resultRegister.Should().Be(serviceRegister);

        registeredServices.Should().HaveCount(1);
        registeredServices.TryGetValue(typeof(ITestService), out var instanceInfoDictionary).Should().BeTrue();
        instanceInfoDictionary.Should().HaveCount(1);
        instanceInfoDictionary!.TryGetValue(typeof(TestImplementServiceWithConstructor), out var info).Should().BeTrue();
        info!.InstanceType.Should().Be(InstanceType.Transient);
        ((TestImplementServiceWithConstructor)info.InstanceFunc!(new TestServiceResolver())).Key.Should().Be(expectedKey);
    }

    public interface ITestService
    {
        
    }

    public class TestImplementService : ITestService
    {
        
    }

    public class TestImplementServiceWithConstructor : ITestService
    {
        public string Key { get; }
        public TestImplementServiceWithConstructor(string key)
        {
            Key = key;
        }
    }

    public class TestServiceResolver : IServiceResolver
    {
        public T ResolveService<T>()
        {
            throw new NotImplementedException();
        }

        public object ResolveService(Type requireServiceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ResolveServices<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveServices(Type requireServiceType)
        {
            throw new NotImplementedException();
        }
    }
}