using CurseTheBeast.Api.Azul;
using CurseTheBeast.Api.Mojang;
using CurseTheBeast.ServerInstaller;
using CurseTheBeast.Services.Model;
using CurseTheBeast.Storage;

namespace CurseTheBeast.Services;


public class ServerModLoaderService : IDisposable
{
    // 此版本的8在绝大多数情况下不会有问题
    const string DefaultJava8Version = "8u312";

    public bool PreinstallSupported { get; }

    readonly AbstractModServerInstaller? _installer = null!;
    readonly FTBModpack _pack;
    readonly bool _preinstall;

    JavaRuntime? _jre;

    public ServerModLoaderService(FTBModpack pack, bool preinstall)
    {
        _pack = pack;
        _preinstall = preinstall;
        _installer = GetInstaller();
        PreinstallSupported = _installer?.IsPreinstallationSupported() ?? false;
    }

    public async Task<IReadOnlyCollection<FileEntry>> GetModLoaderFilesAsync(CancellationToken ct = default)
    {
        // 不预安装，只带一个jar
        if (!_preinstall)
            return await GetStandaloneLoaderJarAsync(ct);

        if (!PreinstallSupported || _installer == null)
            throw new Exception($"不支持预安装{_pack.Runtime.ModLoaderType}-{_pack.Runtime.GameVersion}-{_pack.Runtime.ModLoaderVersion}服务端");

        if(_jre == null)
            _jre = await GetJreZipFileAsync(ct);
        var serverJar = await GetServerJarAsync(ct);
        var loaderFiles = await GetModLoaderFilesAsync(_jre, serverJar, ct);

        Success.WriteLine("√ 服务端预安装完成");
        return loaderFiles;
    }

    public async Task<IReadOnlyCollection<FileEntry>> GetStandaloneLoaderJarAsync(CancellationToken ct)
    {
        if (_installer != null)
        {
            var installerJar = await Focused.StatusAsync($"获取{_installer.Name}加载器", async ctx => await _installer.ResolveStandaloneLoaderJarAsync(ct));
            if (installerJar != null)
            {
                try
                {
                    await FileDownloadService.DownloadAsync($"下载{_pack.Runtime.ModLoaderType}加载器", installerJar, ct);
                    return installerJar;
                }
                catch (Exception)
                {

                }
            }
        }
        // 不支持、获取失败、下载失败就不带了，自己下载去
        return Array.Empty<FileEntry>();
    }

    public async Task<IReadOnlyCollection<FileEntry>> GetModLoaderFilesAsync(JavaRuntime jre, FileEntry serverJar, CancellationToken ct = default)
    {
        var installerJar = await Focused.StatusAsync($"解析{_installer!.Name}安装器", async ctx => await _installer.ResolveInstallerAsync(ct));
        if (installerJar.Count > 0)
            await FileDownloadService.DownloadAsync($"下载{_installer.Name}安装器", installerJar, ct);

        var deps = await Focused.StatusAsync($"解析{_installer.Name}依赖", 
            async ctx => await _installer.ResolveInstallerDependenciesAsync(ct));
        if(deps.Count > 0)
            await FileDownloadService.DownloadAsync($"下载{_installer.Name}依赖", deps, ct);

        return await Focused.StatusAsync($"预安装{_installer.Name}服务端", 
            async ctx => await _installer.PreInstallAsync(jre, serverJar));
    }

    public AbstractModServerInstaller? GetInstaller()
	{
        var installer = _pack.Runtime.ModLoaderType.ToLower() switch
        {
            "forge" => new ForgeServerInstaller(),
            "fabric" => new FabricServerInstaller() as AbstractModServerInstaller,
            _ => null,
        };
        if(installer != null)
        {
            installer.GameVersion = _pack.Runtime.GameVersion;
            installer.LoaderVersion = _pack.Runtime.ModLoaderVersion;
            installer.ServerName = $"{_pack.Name} v{_pack.Version.Name} Server";
            installer.Ram = _pack.Runtime.RecommendedRam;
        }
        return installer;
    }

    public async Task<FileEntry> GetServerJarAsync(CancellationToken ct = default)
    {
        var manifest = await Focused.StatusAsync("解析服务端", async ctx =>
        {
            using var api = new MojangApiClient();
            var list = await api.GetGameVersionListAsync(ct);
            var version = list.versions.FirstOrDefault(v => v.id == _pack.Runtime.GameVersion)
                ?? throw new Exception("未知的MC版本：" + _pack.Runtime.GameVersion);
            return await api.GetGameManifestAsync(version.url, ct);
        });
        var serverJarFile = new FileEntry(RepoType.ServerJar, $"{_pack.Runtime.GameVersion}.jar")
            .SetDownloadable($"mc-server-{_pack.Runtime.GameVersion}.jar", manifest.downloads.server.url)
            .WithSha1(manifest.downloads.server.sha1)
            .WithSize(manifest.downloads.server.size);
        await FileDownloadService.DownloadAsync("下载服务端", new[] { serverJarFile }, ct);
        return serverJarFile;
    }

    public async Task<JavaRuntime> GetJreZipFileAsync(CancellationToken ct = default)
    {
        var jreZipFile = await Focused.StatusAsync("获取Java运行环境信息", async ctx =>
        {
            var os = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "windows",
                PlatformID.Unix => "linux",
                _ => throw new NotSupportedException("服务端预安装失败：不支持当前操作系统")
            };
            using var api = new AzulApiClient();
            var manifest = (await api.GetZuluPackageAsync(_pack.Runtime.JavaVersion, os, ct)).FirstOrDefault();
            if (manifest == null)
            {
                var majorVersion = int.Parse(_pack.Runtime.JavaVersion.Substring(0, _pack.Runtime.JavaVersion.IndexOf('.')));
                if (majorVersion == 8)
                {
                    manifest = (await api.GetZuluPackageAsync(DefaultJava8Version, os, ct)).FirstOrDefault();
                }
                else
                {
                    // 下载指定majorVersion的最新版
                    manifest = (await api.GetZuluPackageAsync(majorVersion.ToString(), os, ct)).FirstOrDefault();
                }
                if (manifest == null)
                {
                    throw new Exception($"服务端预安装失败：无法获取Java{_pack.Runtime.JavaVersion}运行环境信息");
                }
            }

            var versionString = string.Join('.', manifest.java_version);
            var fileName = $"zulu-{_pack.Runtime.JavaVersion}-{os}.zip";
            return new FileEntry(RepoType.JreArchive, fileName)
                .SetDownloadable(fileName, manifest.download_url);
        });
        await FileDownloadService.DownloadAsync("下载Java运行环境", new[] { jreZipFile }, ct);

        return Focused.Status("准备Java运行环境", ctx =>
        {
            return JavaRuntime.FromZip(jreZipFile.LocalPath);
        });
    }

    public void Dispose()
    {
        _installer?.Dispose();
        _jre?.Dispose();
    }
}