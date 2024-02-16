using Spectre.Console;

namespace SingleCommand;

public interface IGreeter
{
    void Greet(string name);
}

public sealed class HelloWorldGreeter : IGreeter
{
    public void Greet(string name)
    {
        var time = Program.Stopwatch.Elapsed.TotalMilliseconds;
        AnsiConsole.MarkupLine($"[yellow]Hello[/] [green]{name}[/]! [grey](in {time}ms)[/]");
    }
}
