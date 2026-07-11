using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace Spectre.Console.Extensions.Hosting.Infrastructure;

public interface IHostEnabledCommandApp : ICommandApp
{
    void SetHost(IHost host);
}
