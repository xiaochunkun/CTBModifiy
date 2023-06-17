using CurseTheBeast.Commands.Options;
using CurseTheBeast.Services;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace CurseTheBeast.Commands;

[Description("列出所有整合包")]
public class ListCommand : AsyncCommand<HttpOptions>
{
    public override async Task<int> ExecuteAsync(CommandContext context, HttpOptions options)
    {
        HttpConfigService.SetupHttp(options);
        using var ftb = new FTBService();

        await ftb.ListAsync(false, default);

        return 0;
    }
}
