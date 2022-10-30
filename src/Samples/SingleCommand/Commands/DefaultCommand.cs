using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace SingleCommand.Commands;

public sealed class DefaultCommand : Command<DefaultCommand.Settings>
{
    private readonly IGreeter _greeter;

    public DefaultCommand(IGreeter greeter)
    {
        _greeter = greeter ?? throw new ArgumentNullException(nameof(greeter));
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        _greeter.Greet(settings.Name);
        return 0;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-n|--name <NAME>")]
        [Description("The person or thing to greet.")]
        [DefaultValue("World")]
        public string Name { get; set; } = string.Empty;
    }
}
