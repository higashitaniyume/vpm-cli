using System.ComponentModel;
using System.Text.Json;
using PackageManager.Core;
using PackageManager.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackageManager.Cli.Commands;

public class PublishCommand : AsyncCommand<PublishCommand.Settings>
{
    public class Settings : ApiSettings
    {
        [CommandArgument(0, "<MANIFEST_PATH>")]
        [Description("Path to the package manifest JSON file.")]
        public string ManifestPath { get; set; } = string.Empty;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!File.Exists(settings.ManifestPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Manifest file not found at {settings.ManifestPath}");
            return 1;
        }

        PublishPackageRequest? request;
        try
        {
            var json = await File.ReadAllTextAsync(settings.ManifestPath);
            request = JsonSerializer.Deserialize<PublishPackageRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error parsing manifest:[/] {ex.Message}");
            return 1;
        }

        if (request == null)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Invalid manifest format.");
            return 1;
        }

        using var client = new ApiClient(settings.ApiUrl);
        ApiResponse<string>? result = null;

        await AnsiConsole.Status().StartAsync("Publishing package...", async ctx =>
        {
            result = await client.PublishPackageAsync(request);
        });

        if (result?.Success == true)
        {
            AnsiConsole.MarkupLine($"[bold green]Successfully published[/] [blue]{request.Namespace}/{request.Name}[/] version [green]{request.Version}[/]");
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold red]Failed to publish:[/] {result?.Message ?? "Unknown error"}");
            return 1;
        }
    }
}

public class ValidateCommand : Command<ValidateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<MANIFEST_PATH>")]
        [Description("Path to the package manifest JSON file.")]
        public string ManifestPath { get; set; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!File.Exists(settings.ManifestPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File not found: {settings.ManifestPath}");
            return 1;
        }

        try
        {
            var json = File.ReadAllText(settings.ManifestPath);
            var request = JsonSerializer.Deserialize<PublishPackageRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Namespace) || string.IsNullOrEmpty(request.Version))
            {
                AnsiConsole.MarkupLine("[red]Invalid manifest:[/] Missing required fields (namespace, name, version).");
                return 1;
            }

            AnsiConsole.MarkupLine("[bold green]Manifest is valid![/]");
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Field");
            table.AddColumn("Value");
            table.AddRow("Namespace", request.Namespace);
            table.AddRow("Name", request.Name);
            table.AddRow("Version", request.Version);
            table.AddRow("Installer URL", request.InstallerUrl);
            table.AddRow("File Hash", request.FileHash);
            
            AnsiConsole.Write(table);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Invalid manifest JSON:[/] {ex.Message}");
            return 1;
        }
    }
}
