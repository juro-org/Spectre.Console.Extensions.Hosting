using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace Spectre.Console.Extensions.Hosting.Infrastructure;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder = new ServiceCollection();
    private IServiceCollection _hostServiceCollection = null!;
    private IHost? _host;

    public TypeRegistrar(IHostBuilder builder)
    {
        builder.ConfigureServices((_, serviceCollection) =>
        {
            _hostServiceCollection = serviceCollection;
        });
    }

    public void SetHost(IHost host)
    {
        _host = host;
    }

    public ITypeResolver Build()
    {
        if (_host == null)
        {
            throw new NotSupportedException("SetHost must be called before the Resolver can be accessed.");
        }

        // copy all registrations from the host ServiceCollection to our internal ServiceCollection,
        // so we have them all available, but do not modify the host ServiceCollection ourselves.
        foreach (var serviceDescriptor in _hostServiceCollection)
        {
            var type = serviceDescriptor.ServiceType;
            if (serviceDescriptor.ImplementationType != null)
            {
                _builder.AddSingleton(type, serviceDescriptor.ImplementationType);
                continue;
            }

            _builder.AddSingleton(type, _ => _host.Services.GetService(type)!);
        }

        return new TypeResolver(_builder.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> func)
    {
        if (func is null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        _builder.AddSingleton(service, _ => func());
    }
}
