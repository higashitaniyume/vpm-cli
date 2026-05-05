using System.ComponentModel;
using PackageManager.Core;
using PackageManager.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackageManager.Cli.Commands;

public class InstallCommand : AsyncCommand<InstallCommand.Settings>
{
    public class Settings : ApiSettings
    {
        [CommandArgument(0, "<PACKAGE_ID>")]
        [Description("The package ID (e.g., @user/app).")]
        public string PackageId { get; set; } = string.Empty;

        [CommandOption("-v|--version <VERSION>")]
        [Description("Specific version to install.")]
        public string? Version { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var (ns, name, inputVersion) = PackageIdParser.Parse(settings.PackageId);
        var version = settings.Version ?? inputVersion;

        using var client = new ApiClient(settings.ApiUrl);
        
        // 1. Get version details
        PackageVersion? verDetails = null;
        await AnsiConsole.Status().StartAsync("Fetching package details...", async ctx => 
        {
            if (string.IsNullOrEmpty(version))
            {
                var pkg = await client.GetPackageAsync(ns, name);
                if (pkg != null && pkg.Versions.Any())
                {
                    version = pkg.Versions.OrderByDescending(v => v.CreatedAt).First().Version;
                }
            }

            if (!string.IsNullOrEmpty(version))
            {
                verDetails = await client.GetPackageVersionAsync(ns, name, version);
            }
        });

        if (verDetails == null)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Could not find version {version ?? "latest"} for {ns}/{name}.");
            return 1;
        }

        AnsiConsole.MarkupLine($"Installing [blue]{ns}/{name}[/] version [green]{verDetails.Version}[/]");

        // 2. Download
        var fileName = Path.GetFileName(new Uri(verDetails.InstallerUrl).LocalPath);
        if (string.IsNullOrEmpty(fileName)) fileName = $"{name}-{verDetails.Version}.exe";
        
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        var downloader = new Downloader();

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[green]Downloading {fileName}[/]");
                await downloader.DownloadFileAsync(verDetails.InstallerUrl, tempPath, (read, total) =>
                {
                    if (total.HasValue)
                    {
                        task.MaxValue = total.Value;
                        task.Value = read;
                    }
                });
            });

        // 3. Verify Hash
        AnsiConsole.MarkupLine("Verifying file hash...");
        var isValid = await Task.Run(() => HashValidator.Validate(tempPath, verDetails.FileHash));

        if (isValid)
        {
            AnsiConsole.MarkupLine("[bold green]Success:[/] Hash matches. Package downloaded to " + tempPath);
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]Error:[/] Hash mismatch! Deleting corrupted file.");
            if (File.Exists(tempPath)) File.Delete(tempPath);
            return 1;
        }

        return 0;
    }
}
