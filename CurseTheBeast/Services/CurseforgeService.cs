using CurseTheBeast.Api.Curseforge;
using CurseTheBeast.Services.Model;
using CurseTheBeast.Utils;

namespace CurseTheBeast.Services;


public class CurseforgeService
{
    public static async Task<IEnumerable<FTBFileEntry>> GetFilesWithIncorrectMetadata(IEnumerable<FTBFileEntry> allFiles, CancellationToken ct = default)
    {
        return await Focused.StatusAsync($"检查 Curseforge FileID", async ctx =>
        {
            var dict = allFiles.Where(file => file.Curseforge != null)
                .ToDictionary(f => f.Curseforge!.FileId);
            if (dict.Count == 0)
                return [];

            var count = dict.Count;

            using var api = new CurseforgeApiClient();
            var progressed = 0;
            foreach (var batch in dict.Keys.Chunk(50).ToArray())
            {
                ctx.Status = Focused.Text($"检查 Curseforge FileID - {progressed}/{count}");

                var rsp = await api.GetFilesAsync(batch, ct);
                foreach (var rspFile in rsp.DistinctBy(f => f.id))
                {
                    if (!dict.TryGetValue(rspFile.id, out var file))
                        continue;

                    if (string.IsNullOrWhiteSpace(file.Sha1) || file.Sha1 != rspFile.hashes.Where(h => h.algo == 1).FirstOrDefault()?.value)
                        continue;

                    dict.Remove(rspFile.id);
                }

                progressed += rsp.Length;
            }

            return dict.Values as IEnumerable<FTBFileEntry>;
        });
    }
}
