# Spectre.Console.Extensions.Hosting

[![standard-readme compliant][]][standard-readme]
[![Contributor Covenant][contrib-covenantimg]][contrib-covenant]
[![Build][githubimage]][githubbuild]
[![NuGet package][nugetimage]][nuget]

Adds [Spectre.Console CommandApp][SpectreConsoleDokuCommandApp] extensions for Microsoft.Extensions.Hosting. 

## Table of Contents

- [Install](#install)
- [Usage](#usage)
- [Examples](#examples)
- [Maintainer](#maintainer)
- [Contributing](#contributing)
- [License](#license)

## Install

Install the Spectre.Console.Extensions.Hosting NuGet package into your app.

```powershell
Install-Package Spectre.Console.Extensions.Hosting
```
## Usage

After the installation you can use the extension ```UseSpectreConsole``` for IHostBuilder.
For this you must configure the SpectreConsole CommandApp in your application`s __Program.cs__ file:

```csharp
 public static async Task<int> Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .UseSpectreConsole<DefaultCommand>()
            .ConfigureServices(
                (_, services) => { services.AddSingleton<IGreeter, HelloWorldGreeter>(); })
            .RunConsoleAsync();
        return Environment.ExitCode;
    }
```

It is also possible to customize the command configurations:

```csharp
    Host.CreateDefaultBuilder(args)
        ...
        .UseSpectreConsole(config =>
        {
            config.AddCommand<AddCommand>("add");
            config.AddCommand<CommitCommand>("commit");
            config.AddCommand<RebaseCommand>("rebase");
#if DEBUG
            config.PropagateExceptions();
            config.ValidateExamples();
#endif
        })
        ...
```

## Examples 

Examples are located in the [samples] folder. 
It is planned to further adopt CLI examples from [Spectre.Console].

## Maintainer

[Jürgen Rosenthal-Buroh @JuergenRB][maintainer]

## Contributing

Spectre.Console.Extensions.Hosting follows the [Contributor Covenant][contrib-covenant] Code of Conduct.

We accept Pull Requests.

Small note: If editing the Readme, please conform to the [standard-readme][] specification.

## License

[MIT License © Jürgen Rosenthal-Buroh][license]

[contrib-covenant]: https://www.contributor-covenant.org/version/1/4/code-of-conduct
[contrib-covenantimg]: https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg
[maintainer]: https://github.com/JuergenRB
[nuget]: https://nuget.org/packages/Spectre.Console.Extensions.Hosting
[nugetimage]: https://img.shields.io/nuget/v/Spectre.Console.Extensions.Hosting.svg?logo=nuget&style=flat-square
[license]: LICENSE.txt
[standard-readme]: https://github.com/RichardLitt/standard-readme
[standard-readme compliant]: https://img.shields.io/badge/readme%20style-standard-brightgreen.svg?style=flat-square
[Spectre.Console]: https://github.com/spectreconsole/spectre.console
[SpectreConsoleDokuCommandApp]: https://spectreconsole.net/cli/commandapp
[samples]: https://github.com/juro-org/Spectre.Console.Extensions.Hosting/tree/develop/src/Samples/
[githubbuild]: https://github.com/juro-org/Spectre.Console.Extensions.Hosting/actions/workflows/build.yml?query=branch%3Adevelop
[githubimage]: https://github.com/juro-org/Spectre.Console.Extensions.Hosting/actions/workflows/build.yml/badge.svg?branch=develop