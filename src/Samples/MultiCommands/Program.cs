using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiCommand.Commands;
using MultiCommand.Commands.Add;
using MultiCommand.Commands.Run;
using MultiCommand.Commands.Serve;
using Spectre.Console.Cli;
using Spectre.Console.Extensions.Hosting;

namespace MultiCommand;

public static class Program
{
    //Example files taken from https://github.com/spectreconsole/spectre.console/tree/main/examples/Cli/Demo and adapted.
    public static async Task<int> Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .UseSpectreConsole(config =>
                {
                    config.SetApplicationName("fake-dotnet");
                    config.ValidateExamples();
                    config.AddExample(new[] { "run", "--no-build" });

                    // Run
                    config.AddCommand<RunCommand>("run");

                    // Add
                    config.AddBranch<AddSettings>("add", add =>
                    {
                        add.SetDescription("Add a package or reference to a .NET project");
                        add.AddCommand<AddPackageCommand>("package");
                        add.AddCommand<AddReferenceCommand>("reference");
                    });

                    // Serve
                    config.AddCommand<ServeCommand>("serve")
                        .WithExample(new[] { "serve", "-o", "firefox" })
                        .WithExample(new[] { "serve", "--port", "80", "-o", "firefox" });
                })
            .RunConsoleAsync();
        return Environment.ExitCode;
    }
}
