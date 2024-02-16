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

        builder.ConfigureServices((_, collection) =>
            {
                collection.AddSingleton<ICommandApp>(sp =>
                {
                    var scope = sp.CreateScope();
                    var registrar = TieredTypeRegistrar.FromServices(scope.ServiceProvider);

                    var command = new CommandApp(registrar);
                    command.Configure(configureCommandApp);

                    return command;
                });
            }
        );

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

        builder.ConfigureServices((_, collection) =>
            {
                collection.AddSingleton<ICommandApp>(sp =>
                {
                    var scope = sp.CreateScope();
                    var registrar = TieredTypeRegistrar.FromServices(scope.ServiceProvider);

                    var command = new CommandApp<TDefaultCommand>(registrar);
                    if (configureCommandApp != null)
                    {
                        command.Configure(configureCommandApp);
                    }

                    return command;
                });

                collection.AddHostedService<SpectreConsoleWorker>();
            }
        );

        return builder;
    }
}
