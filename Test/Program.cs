using Epinephrine;

var register = new ServiceRegister();

register.RegisterTransient<IService, Service>(x => new Service("Dependency Injection"));
register.RegisterTransient<IService2, Service2>();
register.RegisterTransient<IService3, Service3>();
var resolver = register.CreateResolver();

var res = resolver as ServiceResolver;
var service3 = res!.ResolveService<IService3>();
Console.WriteLine(service3.GetService().GetService().Key);

Console.ReadKey();


public interface IService
{
    string Key { get; }
}

public interface IService2
{
    IService GetService();
}

public interface IService3
{
    IService2 GetService();
}

public class Service : IService
{
    public string Key { get; }

    public Service(string key)
    {
        Key = key;
    }
}

public class Service2 : IService2
{
    private readonly IService _service;
    public Service2(IService service)
    {
        _service = service;
    }
    public IService GetService()
    {
        return _service;
    }
}

public class Service3 : IService3
{
    private readonly IService2 _service;
    public Service3(IService2 service)
    {
        _service = service;
    }
    public IService2 GetService()
    {
        return _service;
    }
}