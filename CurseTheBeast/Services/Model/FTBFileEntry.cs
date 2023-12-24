using CurseTheBeast.Api.FTB.Model;
using CurseTheBeast.Storage;
using CurseTheBeast.Utils;

namespace CurseTheBeast.Services.Model;


public class FTBFileEntry : FileEntry
{
    public long Id { get; }
    public FileSide Side { get; }
    public CurseforgeInfo? Curseforge { get; }

    public FTBFileEntry(ModpackManifest.File file)
        : base(RepoType.Asset, getAssetCachePath(file.id))
    {
        Id = file.id;
        WithSha1(file.sha1);
        WithSize(file.size);
        WithArchiveEntryName(file.path, file.name);

        if (ArchiveEntryName!.StartsWith("mods/") && ArchiveEntryName.EndsWith(".jar.disabled"))
            ArchiveEntryName = ArchiveEntryName.Remove(ArchiveEntryName.Length - 9);

        var url = !string.IsNullOrWhiteSpace(file.url) ? file.url
           : CurseforgeUtils.GetDownloadUrl(file.curseforge!.file,file.name);
        SetDownloadable(file.name, url);
        // 有些mod删库跑路，跳过下载交给用户处理
        SetUnrequired();

        Side = file switch
        {
            { serveronly: true } => FileSide.Server,
            { clientonly: true } => FileSide.Client,
            _ => FileSide.Both,
        };
        Curseforge = file.curseforge == null ? null : new CurseforgeInfo()
        {
            ProjectId = file.curseforge.project,
            FileId = file.curseforge.file
        };
    }

    static string[] getAssetCachePath(long id)
    {
        return new[] { ((byte)(id & 0xFF)).ToString("x2"), id.ToString("x2") };
    }

    public class CurseforgeInfo
    {
        public long ProjectId { get; init; }
        public long FileId { get; init; }
    }

    [Flags]
    public enum FileSide
    {
        Client = 1,
        Server = 2,
        Both = Client | Server
    }
}
