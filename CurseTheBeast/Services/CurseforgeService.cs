using CurseTheBeast.Api.Curseforge;
using CurseTheBeast.Services.Model;
using CurseTheBeast.Utils;

namespace CurseTheBeast.Services;


public class CurseforgeService
{
    public static async Task TryRecoverUnreachableFiles(IEnumerable<FTBFileEntry> allFiles, CancellationToken ct = default)
    {
        var dict = allFiles.Where(file => file.Unreachable && file.Curseforge != null)
            .ToDictionary(f => f.Curseforge!.FileId);
        if (dict.Count == 0)
            return;

        await Focused.StatusAsync("尝试恢复失效的文件", async ctx =>
        {
            using var api = new CurseforgeApiClient();
            foreach (var batch in dict.Keys.Chunk(50))
            {
                try
                {
                    var rsp = await api.GetFilesAsync(batch, ct);
                    foreach (var rspFile in rsp.DistinctBy(f => f.id))
                    {
                        if (!dict.TryGetValue(rspFile.id, out var file))
                            continue;

                        var url = !string.IsNullOrWhiteSpace(rspFile.downloadUrl) ? rspFile.downloadUrl
                            : CurseforgeUtils.GetDownloadUrl(rspFile.id, rspFile.fileName);
                        file.WithSize(rspFile.fileLength)
                            .WithSha1(rspFile.hashes.Where(h => h.algo == 1).FirstOrDefault()?.value)
                            .SetDownloadable(rspFile.fileName, url);
                    }
                }
                catch(Exception)
                {
                    continue;
                }
            }
        });

        await FileDownloadService.DownloadAsync("重新下载失效文件", dict.Values.Where(f => !f.Unreachable).ToArray(), ct);
    }
}
