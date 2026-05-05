using PackageManager.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("vpm");
    
    config.AddCommand<SearchCommand>("search")
        .WithDescription("Search for packages by keyword")
        .WithExample("search", "browser");

    config.AddCommand<InfoCommand>("info")
        .WithDescription("Get detailed information about a package")
        .WithExample("info", "@google/chrome");

    config.AddCommand<InstallCommand>("install")
        .WithDescription("Download and verify a package")
        .WithExample("install", "@google/chrome")
        .WithExample("install", "@google/chrome:123.0.0");

    config.AddCommand<PublishCommand>("publish")
        .WithDescription("Publish a new package version to the registry")
        .WithExample("publish", "manifest.json");

    config.AddCommand<ValidateCommand>("validate")
        .WithDescription("Validate a package manifest JSON file")
        .WithExample("validate", "manifest.json");
});

return await app.RunAsync(args);
