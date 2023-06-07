using CurseTheBeast.Api.FTB.Model;
using CurseTheBeast.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CurseTheBeast.Commands;


public class DefaultCommand : AsyncCommand
{
    readonly FTBService _ftb;

    public DefaultCommand()
    {
        HttpConfigService.SetupHttpProxy(false, null);
        _ftb = new FTBService();
    }

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var op = Prompt("按上下键选择，回车确认:",
            "查看热门整合包",
            "搜索整合包",
            "手动输入整合包ID");

        switch(op)
        {
            case 0: return await SelectFeaturedModpack();
            case 1: return await SearchModpack();
            case 2: return await InputIds();
            default:return -1;
        }
    }

    async Task<int> SelectFeaturedModpack(CancellationToken ct = default)
    {
        var packs = await _ftb.GetFeaturedModpacksAsync();
        return await SelectModpack(packs, ct);
    }

    async Task<int> SearchModpack(CancellationToken ct = default)
    {
        var keyword = AnsiConsole.Ask<string>(Focused.Text(Environment.NewLine + "输入关键词:")).Trim();
        var result = await _ftb.SearchAsync(keyword, ct);

        if(result.Count == 0)
        {
            Error.WriteLine("搜索结果为空");
            return 1;
        }
        return await SelectModpack(result, ct);
    }

    async Task<int> SelectModpack(IReadOnlyList<(int Id, string Name, int update)> packs, CancellationToken ct)
    {
        if (packs.Count > 1)
        {
            packs = packs.OrderByDescending(p => p.update).ToArray();
            var index = Prompt("选择整合包:", packs.Select(p => $"{p.Name} （{p.Id}）").ToArray());
            return await SelectVersions(packs[index].Id, ct);
        }
        else
        {
            return await SelectVersions(packs[0].Id, ct);
        }
    }

    async Task<int> InputIds(CancellationToken ct = default)
    {
        var id = AnsiConsole.Ask<int>(Environment.NewLine + Focused.Text("输入整合包ID:"));
        return await SelectVersions(id, ct);
    }

    async Task<int> SelectVersions(int packId, CancellationToken ct)
    {
        var info = await _ftb.GetModpackInfoAsync(packId, ct);
        Success.WriteLine("整合包：" + info.name);

        var versions = info.versions.OrderByDescending(v => v.id).ToArray();
        var index = Prompt("选择整合包版本:", versions.Select(v => v.type.ToLower() switch
            {
                "release" => $"{v.name} 正式版 （{v.id}）",
                "beta" => Shallow.Text($"{v.name} 测试版 （{v.id}）"),
                "alpha" or "archived" => Low.Text($"{v.name} BUG版 （{v.id}）"),
                _ => $"{v.name} {v.type.ToLower()} （{v.id}）",
            }).ToArray());

        Success.WriteLine("版本：" + versions[index].name);
        return await Download(info, versions[index], ct);
    }

    async Task<int> Download(ModpackInfo info, ModpackInfo.Version version, CancellationToken ct)
    {
        var server = Prompt("选择整合包类型:", "客户端 - 用来玩", "服务端 - 用来开服") == 1;
        var full = server || Prompt("选择下载类型:", "标准包 - 下载快，体积小", "完整包 - 安装快") == 1;
        var preinstall = server && Prompt($"是否预安装服务端，并且同意MC用户协议：https://aka.ms/MinecraftEULA", 
            "是，并且同意该协议",
            "否，稍后手动安装") == 0;
        var output = Environment.CurrentDirectory;

        if (server)
            Success.WriteLine("类型：服务端" + (preinstall ? "（预安装）" : ""));
        else
            Success.WriteLine("类型：客户端" + (full ? "（完整包）" : "（标准包）"));

        Success.WriteLine($"保存位置: {output}");
        Focused.WriteLine("");
        
        AnsiConsole.Live(Focused.Markup("按任意键开始下载...")).AutoClear(true).Start(ctx =>
        {
            ctx.Refresh();
            Console.ReadKey();
        });
        
        Focused.WriteLine("");

        var pack = await _ftb.GetModpackAsync(info, version.id, ct);
        await _ftb.DownloadModpackFilesAsync(pack, server, full, ct);

        if (server)
        {
            using var serverLoader = new ServerModLoaderService(pack, preinstall);
            var loaderFiles = await serverLoader.GetModLoaderFilesAsync(ct);
            await PackService.PackServerAsync(pack, loaderFiles, preinstall, output, ct);
        }
        else
        {
            await PackService.PackClientAsync(pack, full, output, ct);
        }

        return 0;
    }

    public int Prompt(string title, params string[] selections)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .MoreChoicesText("(下面有更多选项)")
                .HighlightStyle(Focused)
                .Title(Focused.Text(Environment.NewLine + title))
                .AddChoices(Enumerable.Range(0, selections.Length).ToArray())
                .UseConverter(s => selections[s]));
    }
}
