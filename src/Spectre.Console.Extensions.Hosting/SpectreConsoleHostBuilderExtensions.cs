using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using Spectre.Console.Extensions.Hosting.Infrastructure;
using Spectre.Console.Extensions.Hosting.Worker;

namespace Spectre.Console.Extensions.Hosting;

/// <summary>
///     Extends <see cref="IHostBuilder" /> with SpectreConsole commands.
/// </summary>
public static class SpectreConsoleHostBuilderExtensions
{
    /// <summary>
    ///     Adds a entry point for a command line application with multi commands.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="configureCommandApp">Configures the command line application commands.</param>
    /// <returns>The host builder</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IHostBuilder UseSpectreConsole(this IHostBuilder builder, Action<IConfigurator> configureCommandApp)
    {
        builder = builder ?? throw new ArgumentNullException(nameof(builder));
        var command = new HostEnabledCommandApp(new TypeRegistrar(builder));
        command.Configure(configureCommandApp);

        builder.ConfigureServices((_, collection) =>
        {
            collection.AddSingleton<IHostEnabledCommandApp>(command);
            collection.AddHostedService<SpectreConsoleWorker>();
        });

        return builder;
    }

    /// <summary>
    ///     Adds a entry point for a command line application with a default command.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="configureCommandApp">Configures the command line application.</param>
    /// <typeparam name="TDefaultCommand">The default command.</typeparam>
    /// <returns>The host builder.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IHostBuilder UseSpectreConsole<TDefaultCommand>(this IHostBuilder builder,
        Action<IConfigurator>? configureCommandApp = null)
        where TDefaultCommand : class, ICommand
    {
        builder = builder ?? throw new ArgumentNullException(nameof(builder));

        var command = new HostEnabledCommandApp<TDefaultCommand>(new TypeRegistrar(builder));
        if (configureCommandApp != null)
        {
            command.Configure(configureCommandApp);
        }

        builder.ConfigureServices((_, collection) =>
        {
            collection.AddSingleton<IHostEnabledCommandApp>(command);
            collection.AddHostedService<SpectreConsoleWorker>();
        });

        return builder;
    }
}
