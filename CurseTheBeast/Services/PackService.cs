using CurseTheBeast.Packs;
using CurseTheBeast.Storage;
using CurseTheBeast.Utils;
using FTBPack = CurseTheBeast.Services.Model.FTBModpack;

namespace CurseTheBeast.Services;


public static class PackService
{
    public static async Task PackClientAsync(FTBPack ftbPack, bool full, string output, CancellationToken ct = default)
    {
        if (Directory.Exists(output))
            output = Path.Combine(output, $"{PathUtils.EscapeFileName(ftbPack.Name)} v{ftbPack.Version.Name}{(full ? " Full" : "")}.zip");
        await Focused.StatusAsync("正在打包", async ctx =>
        {
            await using var fs = File.Create(output);
            await ftbPack.PackCurseforgeAsync(fs, full, ct);
        });
        Success.WriteLine($"√ 打包完成：{output}");
        if (ftbPack.Files.ClientFullFiles.Any(f => f.Unreachable))
            Error.WriteLine("警告：部分文件无法下载，请查看“unreachable-files.json”尝试手动下载");
    }

    public static async Task PackServerAsync(FTBPack ftbPack, IReadOnlyCollection<FileEntry> loaderFiles, bool preinstall, string output, CancellationToken ct = default)
    {
        if (Directory.Exists(output))
            output = Path.Combine(output, $"{PathUtils.EscapeFileName(ftbPack.Name)} v{ftbPack.Version.Name} Server{(loaderFiles.Count > 3 ? " Preinstalled": "")}.zip");

        await Focused.StatusAsync("正在打包", async ctx =>
        {
            await using var fs = File.Create(output);
            var serverPack = new ServerModpack(ftbPack, loaderFiles, preinstall);
            await serverPack.PackServerAsync(fs, ct);
        });
        Success.WriteLine($"√ 打包完成：{output}");
        if (ftbPack.Files.ServerFiles.Any(f => f.Unreachable))
            Error.WriteLine("警告：部分文件无法下载，请查看“unreachable-files.json”尝试手动下载");
    }
}
