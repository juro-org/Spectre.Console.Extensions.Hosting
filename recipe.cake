#load nuget:?package=Cake.Recipe&version=3.1.1

Environment.SetVariableNames();

BuildParameters.SetParameters(
  context: Context,
  buildSystem: BuildSystem,
  sourceDirectoryPath: "./src",
  title: "Spectre.Console.Extensions.Hosting",
  masterBranchName: "main",
  repositoryOwner: "juro-org",
  preferredBuildProviderType: BuildProviderType.GitHubActions,
  shouldRunDotNetCorePack: true,
  shouldUseDeterministicBuilds: true
  );

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

Build.RunDotNetCore();
