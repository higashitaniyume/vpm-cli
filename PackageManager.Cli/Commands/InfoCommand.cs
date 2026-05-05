using System.ComponentModel;
using PackageManager.Core;
using PackageManager.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackageManager.Cli.Commands;

public class InfoCommand : AsyncCommand<InfoCommand.Settings>
{
    public class Settings : ApiSettings
    {
        [CommandArgument(0, "<PACKAGE_ID>")]
        [Description("The package ID (e.g., @user/app).")]
        public string PackageId { get; set; } = string.Empty;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var (ns, name, _) = PackageIdParser.Parse(settings.PackageId);
        using var client = new ApiClient(settings.ApiUrl);

        await AnsiConsole.Status()
            .StartAsync($"Fetching info for {settings.PackageId}...", async ctx =>
            {
                var pkg = await client.GetPackageAsync(ns, name);

                if (pkg == null)
                {
                    AnsiConsole.MarkupLine($"[red]Package {settings.PackageId} not found.[/]");
                    return;
                }

                AnsiConsole.Write(new Rule($"[blue]{pkg.Namespace}/{pkg.Name}[/]") { Justification = Justify.Left });
                AnsiConsole.MarkupLine($"[grey]Description:[/] {pkg.Description ?? "No description available"}");
                AnsiConsole.WriteLine();

                var table = new Table()
                    .Border(TableBorder.Minimal)
                    .AddColumn("Version")
                    .AddColumn("Release Date")
                    .AddColumn("Notes");

                foreach (var v in pkg.Versions.OrderByDescending(x => x.CreatedAt))
                {
                    table.AddRow(
                        $"[green]{v.Version}[/]",
                        v.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                        v.ReleaseNotes ?? ""
                    );
                }

                AnsiConsole.Write(table);
            });

        return 0;
    }
}
