using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Spectre.Console.Extensions.Hosting.Worker;

public class SpectreConsoleWorker : IHostedService
{
    private readonly ICommandApp _commandApp;
    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly ILogger<SpectreConsoleWorker> _logger;
    private int _exitCode;

    public SpectreConsoleWorker(ILogger<SpectreConsoleWorker> logger, ICommandApp commandApp,
        IHostApplicationLifetime hostLifetime)
    {
        _logger = logger;
        _commandApp = commandApp;
        _hostLifetime = hostLifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            try
            {
                var args = GetArgs();
                await Task.Delay(100, cancellationToken); //Just to let Microsoft.Hosting finish.
                _exitCode = await _commandApp.RunAsync(args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                _exitCode = 1;
            }
            finally
            {
                _hostLifetime.StopApplication();
            }
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Environment.ExitCode = _exitCode;
        return Task.CompletedTask;
    }

    private static string[] GetArgs()
    {
        //Remove path from command line args
        return Environment.GetCommandLineArgs().Skip(1).ToArray();
    }
}
