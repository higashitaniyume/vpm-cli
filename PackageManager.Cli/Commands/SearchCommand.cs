using System.ComponentModel;
using System.Text.Json;
using PackageManager.Core;
using PackageManager.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackageManager.Cli.Commands;

public class SearchCommand : AsyncCommand<SearchCommand.Settings>
{
    public class Settings : ApiSettings
    {
        [CommandArgument(0, "<KEYWORD>")]
        [Description("The keyword to search for.")]
        public string Keyword { get; set; } = string.Empty;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var client = new ApiClient(settings.ApiUrl);
        
        await AnsiConsole.Status()
            .StartAsync("Searching...", async ctx =>
            {
                var results = await client.SearchPackagesAsync(settings.Keyword);

                if (results.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No packages found matching your query.[/]");
                    return;
                }

                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .Title($"Search results for [blue]{settings.Keyword}[/]")
                    .AddColumn("Namespace")
                    .AddColumn("Name")
                    .AddColumn("Description")
                    .AddColumn("Latest Version");

                foreach (var pkg in results)
                {
                    table.AddRow(
                        pkg.Namespace,
                        $"[green]{pkg.Name}[/]",
                        pkg.Description ?? "[grey]N/A[/]",
                        pkg.LatestVersion ?? "[grey]N/A[/]"
                    );
                }

                AnsiConsole.Write(table);
            });

        return 0;
    }
}
