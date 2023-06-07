using Spectre.Console.Cli;
using System.ComponentModel;

namespace CurseTheBeast.Commands.Options;


public class HttpOptions : CommandSettings
{
    [Description("禁用HTTP代理")]
    [CommandOption("-n|--no-proxy")]
    public bool NoProxy { get; init; } = false;

    [Description("指定HTTP代理")]
    [CommandOption("-p|--proxy")]
    public string? Proxy { get; init; } = null!;

    [Description("User agent")]
    [CommandOption("-u|--user-agent")]
    public string? UserAgent { get; init; }
}
