using System.ComponentModel;
using Spectre.Console.Cli;

namespace PackageManager.Cli.Commands;

public class ApiSettings : CommandSettings
{
    [CommandOption("-u|--url <URL>")]
    [Description("The base URL of the package manager API.")]
    [DefaultValue("http://localhost:8787/api")]
    public string ApiUrl { get; set; } = "http://localhost:8787/api";
}
