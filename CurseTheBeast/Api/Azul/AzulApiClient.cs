using CurseTheBeast.Api.Azul.Model;

namespace CurseTheBeast.Api.Azul;


public class AzulApiClient : BaseApiClient
{
    public AzulApiClient()
    {

    }

    public Task<ZuluPackage[]> GetZuluPackageAsync(string javaVersion, string os, CancellationToken ct = default)
    { 
        return GetAsync<ZuluPackage[]>(new Uri($"https://api.azul.com/metadata/v1/zulu/packages/?java_version={javaVersion}&os={os}&arch=x64&archive_type=zip&java_package_type=jre&javafx_bundled=false&release_status=ga&availability_types=CA&page=1&page_size=100"), ZuluPackage.ZuluPackageArrayContext.Default.ZuluPackageArray, ct);
    }
}
