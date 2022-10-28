using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample;
using Sample.Commands;
using Spectre.Console.Extensions.Hosting;

public class Program
{
    //Example files taken from https://github.com/spectreconsole/spectre.console/tree/main/examples/Cli/Injection and adapted.
    public static async Task<int> Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .UseSpectreConsole<DefaultCommand>()
            .ConfigureServices((_, services) => { services.AddSingleton<IGreeter, HelloWorldGreeter>(); })
            .RunConsoleAsync();
        return Environment.ExitCode;
    }
}