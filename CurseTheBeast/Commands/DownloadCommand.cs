using CurseTheBeast.Commands.Options;
using CurseTheBeast.Services;
using CurseTheBeast.Services.Model;
using CurseTheBeast.Utils;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace CurseTheBeast.Commands;


[Description("下载整合包")]
public class DownloadCommand : AsyncCommand<DownloadCommand.Options>
{
    public class Options : DownloadOptions
    {
        [Description("整合包ID")]
        [CommandArgument(0, "<PackId>")]
        public int PackId { get; set; }

        [Description("版本ID（默认最新版）")]
        [CommandArgument(1, "[VersionId]")]
        public int VersionId { get; set; } = 0;
        
        [Description("下载完整客户端（包含Curseforge文件）")]
        [CommandOption("-f|--full")]
        public bool FullPack { get; set; } = true;
    }


    public override async Task<int> ExecuteAsync(CommandContext context, Options options)
    {
        DirectoryUtils.SetupOutputDirectory(options.Output, false);
        HttpConfigService.SetupHttp(options);

        using var ftb = new FTBService();

        FTBModpack pack;
        if(options.VersionId == 0)
        {
            var info = await ftb.GetModpackInfoAsync(options.PackId);
            options.VersionId = info.versions.OrderByDescending(v => v.id).First().id;
            Success.WriteLine($"√ Latest version id {options.VersionId} selected");

            pack = await ftb.GetModpackAsync(info, options.VersionId);
        }
        else
        {
            pack = await ftb.GetModpackAsync(options.PackId, options.VersionId);
        }

        var packType = (options.Server, options.FullPack, options.PreInstall) switch 
        {
            (true, _, true) => "Server Preinstalled",
            (true, _, false) => "Server",
            (false, true, _) => "Client Full",
            (false, false, _) => "Client",
        };

        Success.WriteLine($"√ {pack.Name} v{pack.Version.Name}({pack.Version.Type}) {packType}");
        await ftb.DownloadModpackFilesAsync(pack, options.Server, options.FullPack);

        if(options.Server)
        {
            using var server = new ServerModLoaderService(pack, options.PreInstall);
            var loaderFiles = await server.GetModLoaderFilesAsync();
            await PackService.PackServerAsync(pack, loaderFiles, options.PreInstall, options.Output);
        }
        else
        {
            await PackService.PackClientAsync(pack, options.FullPack, options.Output);
        }

        return 0;
    }
}