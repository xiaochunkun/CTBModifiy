using CurseTheBeast.Commands.Options;
using CurseTheBeast.Mirrors;
using Spectre.Console;
using System.Net;

namespace CurseTheBeast.Services;


public class HttpConfigService
{
    public static IWebProxy? Proxy { get; private set; }
    public static string UserAgent { get; private set; } = $"{AppInfo.Name}/{AppInfo.Version}";
    public static int Thread { get; private set; } = 8;

    public static void SetupHttp(DownloadOptions options)
    {
        if (options.Thread != null)
            Thread = options.Thread.Value;
        SetupHttp(options as HttpOptions);
    }

    public static void SetupHttp(HttpOptions options)
    {
        if (options.UserAgent != null)
            UserAgent = options.UserAgent;
        SetupHttpProxy(options.NoProxy, options.Proxy);
    }

    public static void SetupHttpProxy(bool noProxy, string? proxyUri)
    {
        if (noProxy)
        {
            return;
        }

        if (proxyUri != null)
        {
            Proxy = MirrorManager.WrapWebProxy(new WebProxy(proxyUri));
            Notice.WriteLine("（正在使用命令行指定的代理）");
            AnsiConsole.WriteLine();
            return;
        }

        var proxy = WebRequest.DefaultWebProxy;
        if (proxy?.GetProxy(new Uri("https://api.modpacks.ch/")) != null)
        {
            Proxy = MirrorManager.WrapWebProxy(proxy);
            Notice.WriteLine("（正在使用系统代理）");
            AnsiConsole.WriteLine();

            return;
        }

        proxyUri = Environment.GetEnvironmentVariable("HTTP_PROXY");
        if (proxyUri != null)
        {
            Proxy = MirrorManager.WrapWebProxy(new WebProxy(proxyUri));
            Notice.WriteLine("（正在使用环境变量指定的代理）");
            AnsiConsole.WriteLine();
            return;
        }

        return;
    }
}
