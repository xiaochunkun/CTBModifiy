using CurseTheBeast.Api.FTB.Model;
using System.Text.Json.Serialization.Metadata;

namespace CurseTheBeast.Api.FTB;


public class FTBApiClient : BaseApiClient
{
    const string RspStatusSuccess = "success";

    public FTBApiClient()
    {

    }

    protected override async Task<TRsp> CallJsonAsync<TRsp>(HttpMethod method, Uri uri, Func<HttpContent>? contentProvider, JsonTypeInfo? type, CancellationToken ct)
    {
        var rsp = await base.CallJsonAsync<TRsp>(method, uri, contentProvider, type, ct);
        if (rsp is FTBRsp ftbRsp && ftbRsp.status != null && ftbRsp.status != RspStatusSuccess)
            throw new FTBException(uri.PathAndQuery, ftbRsp.status, ftbRsp.message);
        return rsp;
    }

    public Task<ModpackSearchResult> SearchAsync(string keyword, CancellationToken ct = default)
        => GetAsync<ModpackSearchResult>(new Uri($"https://api.modpacks.ch/public/modpack/search/20/detailed?platform=modpacksch&term={Uri.EscapeDataString(keyword)}"), ModpackSearchResult.ModpackSearchResultContext.Default.ModpackSearchResult, ct);

    public Task<ModpackList> GetListAsync(CancellationToken ct = default)
        => GetAsync<ModpackList>(new Uri($"https://api.modpacks.ch/public/modpack/all"), ModpackList.ModpackListContext.Default.ModpackList, ct);

    public Task<ModpackList> GetFeaturedAsync(CancellationToken ct = default)
        => GetAsync<ModpackList>(new Uri($"https://api.modpacks.ch/public/modpack/featured/20"), ModpackList.ModpackListContext.Default.ModpackList, ct);

    public Task<ModpackInfo> GetInfoAsync(int modpackId, CancellationToken ct = default)
        => GetAsync<ModpackInfo>(new Uri($"https://api.modpacks.ch/public/modpack/{modpackId}"), ModpackInfo.ModpackInfoContext.Default.ModpackInfo, ct);

    public Task<ModpackManifest> GetManifestAsync(int modpackId, int versionId, CancellationToken ct = default)
        => GetAsync<ModpackManifest>(new Uri($"https://api.modpacks.ch/public/modpack/{modpackId}/{versionId}"), ModpackManifest.ModpackManifestContext.Default.ModpackManifest, ct);

    public Task<ModInfo> GetModInfoAsync(string sha1, CancellationToken ct = default)
        => GetAsync<ModInfo>(new Uri($"https://api.modpacks.ch/public/mod/{sha1}"), ModInfo.ModInfoContext.Default.ModInfo, ct);
}
