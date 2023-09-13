using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace Spectre.Console.Extensions.Hosting.Infrastructure;

internal class HostEnabledCommandApp :  IHostEnabledCommandApp
{
    private readonly TypeRegistrar _typeRegistrar;
    private readonly CommandApp _underlyingCommandApp;

    public HostEnabledCommandApp(TypeRegistrar typeRegistrar)
    {
        _typeRegistrar = typeRegistrar;
        _underlyingCommandApp = new CommandApp(typeRegistrar);
    }

    public void Configure(Action<IConfigurator> configuration)
    {
        _underlyingCommandApp.Configure(configuration);
    }

    public int Run(IEnumerable<string> args)
    {
        return _underlyingCommandApp.Run(args);
    }

    public async Task<int> RunAsync(IEnumerable<string> args)
    {
        return await _underlyingCommandApp.RunAsync(args);
    }

    public void SetHost(IHost host)
    {
        _typeRegistrar.SetHost(host);
    }
}

internal class HostEnabledCommandApp<TDefaultCommand> :  IHostEnabledCommandApp
where TDefaultCommand: class, ICommand
{
    private readonly TypeRegistrar _typeRegistrar;
    private readonly CommandApp<TDefaultCommand> _underlyingCommandApp;

    public HostEnabledCommandApp(TypeRegistrar typeRegistrar)
    {
        _typeRegistrar = typeRegistrar;
        _underlyingCommandApp = new CommandApp<TDefaultCommand>(typeRegistrar);
    }

    public void Configure(Action<IConfigurator> configuration)
    {
        _underlyingCommandApp.Configure(configuration);
    }

    public int Run(IEnumerable<string> args)
    {
        return _underlyingCommandApp.Run(args);
    }

    public async Task<int> RunAsync(IEnumerable<string> args)
    {
        return await _underlyingCommandApp.RunAsync(args);
    }

    public void SetHost(IHost host)
    {
        _typeRegistrar.SetHost(host);
    }
}
