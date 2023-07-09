using CurseTheBeast.Api.Curseforge.Model;
using CurseTheBeast.Services;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace CurseTheBeast.Api.Curseforge;


public class CurseforgeApiClient : BaseApiClient
{
    protected override void OnConfigureHttpClient(HttpClient client)
    {
        client.BaseAddress = new Uri("https://api.curseforge.com");
        client.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", HttpConfigService.CurseforgeKey);
    }

    public async ValueTask<ModFile> GetFileAsync(long projectId, long fileId, CancellationToken ct)
    {
        return await GetAsync<ModFile>(new Uri($"/v1/mods/{projectId}/files/{fileId}", UriKind.Relative), null, ct);
    }

    public async ValueTask<ModFile[]> GetFilesAsync(IEnumerable<long> fileIds, CancellationToken ct)
    {
        return await PostJsonAsync<ModFile[]>(new Uri($"/v1/mods/files", UriKind.Relative), new
        {
            // AOT有bug，不要直接传
            fileIds = new JsonArray(fileIds.Select(id => JsonValue.Create(id)).ToArray()),
        }, null, ct);
    }

    protected async override Task<TRsp> CallJsonAsync<TRsp>(HttpMethod method, Uri uri, Func<HttpContent>? contentProvider, JsonTypeInfo? type, CancellationToken ct)
    {
        return (await base.CallJsonAsync<GenericRsp<TRsp>>(method, uri, contentProvider, null, ct)).data;
    }
}
