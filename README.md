# Epinephrine
> Dependency Injection learning memo

## Usage
```csharp
IServiceRegister serviceRegister = new ServiceRegister();
serviceRegister.RegisterSingleton<IMyService, MyService>();
var resolver = serviceRegister.CreateResolver();
var service = resolver.ResolveService<IMyService>();
service.ServiceMethod();
```

## Requirement
.NET 6