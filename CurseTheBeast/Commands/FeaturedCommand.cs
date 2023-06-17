using CurseTheBeast.Commands.Options;
using CurseTheBeast.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace CurseTheBeast.Commands;


[Description("列出热门整合包")]
public class FeaturedCommand : AsyncCommand<HttpOptions>
{
    public override async Task<int> ExecuteAsync(CommandContext context, HttpOptions options)
    {
        HttpConfigService.SetupHttp(options);
        using var ftb = new FTBService();
        var results = await ftb.GetFeaturedModpacksAsync();

        var table = new Table();
        table.AddColumn(new TableColumn("Id"));
        table.AddColumn(new TableColumn("Name"));
        foreach (var item in results.OrderByDescending(item => item.Id))
        {
            table.AddRow(
                new Text(item.Id.ToString()),
                new Text(item.Name.ToString()));
        }
        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        return 0;
    }
}
