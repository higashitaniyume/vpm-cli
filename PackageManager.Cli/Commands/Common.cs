using System.ComponentModel;
using Spectre.Console.Cli;

namespace PackageManager.Cli.Commands;

public class ApiSettings : CommandSettings
{
    [CommandOption("-u|--url <URL>")]
    [Description("The base URL of the package manager API.")]
    [DefaultValue("https://api.vpm.vlnc.top/api")]
    public string ApiUrl { get; set; } = "https://api.vpm.vlnc.top/api";
}
