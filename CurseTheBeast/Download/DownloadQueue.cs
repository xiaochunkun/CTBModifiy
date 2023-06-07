using CurseTheBeast.Storage;
using CurseTheBeast.Mirrors;
using CurseTheBeast.Services;
using CurseTheBeast.Utils;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;

namespace CurseTheBeast.Download;


public class DownloadQueue : IDisposable
{
    public const int TryTimes = 3;
    public const int Timeout = 30;

    public record TaskProgressedEventArgs(int? Total, int Received, int Progressed);

    public event Action<FileEntry>? TaskStarted;
    public event Action<FileEntry, TaskProgressedEventArgs>? TaskProgressed;
    public event Action<FileEntry>? TaskFinished;

    readonly BlockingCollection<HttpClient> _cliPool;

    public DownloadQueue()
    {
        _cliPool = new BlockingCollection<HttpClient>();
    }

    public async Task DownloadAsync(IEnumerable<FileEntry> tasks, CancellationToken ct = default)
    {
        var queue = new ConcurrentQueue<FileEntry>(tasks);
        if (queue.IsEmpty)
            return;

        var thread = Math.Min(queue.Count, HttpConfigService.Thread);
        for (var i = _cliPool.Count; i < thread; ++i)
            _cliPool.Add(getHttpClient(), ct);
        await Task.WhenAll(Enumerable.Range(0, thread)
            .Select(_ => downloadWorker(queue, ct))
            .ToArray());
    }

    async Task downloadWorker(ConcurrentQueue<FileEntry> queue, CancellationToken ct = default)
    {
        using var owner = MemoryPool<byte>.Shared.Rent(1024 * 1024);
        var buffer = owner.Memory;

        while (!ct.IsCancellationRequested)
        {
            if (!queue.TryDequeue(out var file))
                return;

            using var locker = await Locker.AcquireOrWaitAsync(file.LocalPath, ct);
            if (file.Validate())
            {
                TaskFinished?.Invoke(file);
                continue;
            }
            TaskStarted?.Invoke(file);

            await downloadFile(file, buffer, ct);

            if (!file.Unreachable)
            {
                if (file.ValidateTemp())
                {
                    file.ApplyTemp();
                }
                else
                {
                    throw new InvalidDataException($"文件校验失败: {file.Url}");
                }
            }
            TaskFinished?.Invoke(file);
        }
    }

    async ValueTask downloadFile(FileEntry file, Memory<byte> buffer, CancellationToken ct)
    {

        if (file.Url == null)
            throw new Exception($"文件{file.DisplayName ?? file.LocalPath}无Url，无法下载");

        var uriList = MirrorManager.GetUrls(new Uri(file.Url!)).ToArray();
        for (var i = 1; ; i++)
        {
            var cli = _cliPool.Take(ct);
            try
            {
                using var rsp = await cli.GetAsync(uriList[Math.Min(uriList.Length, i) - 1], HttpCompletionOption.ResponseHeadersRead, ct);
                rsp.EnsureSuccessStatusCode();
                using var rspStream = rsp.Content.ReadAsStream(ct);
                using var fs = File.Create(file.LocalTempPath);
                var received = 0;
                var size = rsp.Content.Headers.ContentLength == null ? file.Size : (int)rsp.Content.Headers.ContentLength.Value;
                if (size != null && size != file.Size)
                    file.WithSize(size.Value);

                while (true)
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(5));
                    var progressed = await rspStream.ReadAsync(buffer, cts.Token);
                    if (progressed == 0)
                        break;
                    received += progressed;
                    TaskProgressed?.Invoke(file, new(size, received, progressed));
                    await fs.WriteAsync(buffer[..progressed], ct);
                }
                break;
            }
            catch (Exception ex)
            {
                file.DeleteTemp();

                if (ct.IsCancellationRequested)
                    throw;

                if (i < uriList.Length)
                    continue;

                if (ex is HttpRequestException hre)
                {
                    // 400系状态码视为unreachable
                    if (hre.StatusCode != null && ((int)hre.StatusCode.Value) / 100 == 4)
                    {
                        if (file.Required)
                            throw new Exception($"下载失败（${(int)hre.StatusCode}）: {file.Url}", ex);
                        else
                            file.SetUnreachable();
                        break;
                    }
                }

                cli.Dispose();
                cli = getHttpClient();
                if (i >= TryTimes)
                    throw new Exception($"下载失败: {file.Url}", ex);
                else
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
            finally
            {
                _cliPool.Add(cli);
            }
        }
    }

    HttpClient getHttpClient()
    {
        var cli = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 3,
            AutomaticDecompression = System.Net.DecompressionMethods.All,
            UseProxy = HttpConfigService.Proxy != null,
            Proxy = HttpConfigService.Proxy,
        })
        {
            Timeout = TimeSpan.FromSeconds(Timeout),
            DefaultRequestVersion = HttpVersion.Version11
        };
        cli.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2");
        cli.DefaultRequestHeaders.UserAgent.ParseAdd(HttpConfigService.UserAgent);
        cli.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh_CN");
        cli.DefaultRequestHeaders.CacheControl = new() { NoCache = true };
        cli.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
        cli.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");
        cli.DefaultRequestHeaders.AcceptEncoding.ParseAdd("br, gzip, deflate");
        return cli;
    }

    public void Dispose()
    {
        while (_cliPool.TryTake(out var cli))
            cli.Dispose();
    }
}
