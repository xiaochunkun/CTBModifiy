namespace CurseTheBeast.Api.NeoForge;


public class NeoForgeApiClient : BaseApiClient
{
    public NeoForgeApiClient()
    {

    }

    public async Task<string?> GetServerInstallerUrlAsync(string gameVersion, string forgeVersion, CancellationToken ct = default)
    {
        var url = $"https://maven.neoforged.net/net/neoforged/forge/{gameVersion}-{forgeVersion}/forge-{gameVersion}-{forgeVersion}-installer.jar";
        if (await IsAvailableAsync(new Uri(url), ct))
            return url;

        return null;
    }
}
