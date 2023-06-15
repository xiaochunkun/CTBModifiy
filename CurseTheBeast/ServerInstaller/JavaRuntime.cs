using CurseTheBeast.Storage;
using System.Diagnostics;
using System.IO.Compression;

namespace CurseTheBeast.ServerInstaller;


public class JavaRuntime : IDisposable
{
    public string DistName { get; }
    public string JavaHome { get; }
    public string JavaPath { get; }
    public string JavaExe { get; }

    readonly LocalStorage _storage;

    public JavaRuntime(LocalStorage storage)
    {
        _storage = storage;
        var dirs = Directory.GetDirectories(storage.WorkSpace);
        if (dirs.Length == 0)
            throw new Exception("无法创建Java运行环境");

        if (dirs.Length == 1)
        {
            DistName = Path.GetFileName(dirs[0]);
            JavaHome = dirs[0];
        }
        else
        {
            DistName = "zulu-jre-x64";
            JavaHome = Path.GetDirectoryName(storage.WorkSpace)!;
        }

        JavaPath = Path.Combine(JavaHome, "bin");
        if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            JavaExe = Path.Combine(JavaPath, "java.exe");
        else
            JavaExe = Path.Combine(JavaPath, "java");
    }

    public static JavaRuntime FromZip(string jreZipPath)
    {
        var storage = LocalStorage.GetTempStorage("java");
        try
        {
            ZipFile.ExtractToDirectory(jreZipPath, storage.WorkSpace, true);
        }
        catch(Exception)
        {
            storage.Dispose();
            throw;
        }
        return new JavaRuntime(storage);
    }

    public async Task<int> ExecuteAsync(string jarPath, IEnumerable<string>? args, string workDir, IReadOnlyDictionary<string, string>? env, CancellationToken ct = default)
    {
        using var p = new Process();

        p.StartInfo.FileName = JavaExe;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.ArgumentList.Add("-jar");
        p.StartInfo.ArgumentList.Add(jarPath);
        if (args != null)
            foreach (var arg in args)
                p.StartInfo.ArgumentList.Add(arg);
        p.StartInfo.WorkingDirectory = workDir;
        p.StartInfo.EnvironmentVariables["JAVA_HOME"] = JavaHome;
        p.StartInfo.EnvironmentVariables["PATH"] = $"{JavaPath}{Path.PathSeparator}{Environment.GetEnvironmentVariable("PATH")}";
        if (env != null)
            foreach (var (k, v) in env)
                p.StartInfo.EnvironmentVariables[k] = v;

        p.Start();
        p.BeginErrorReadLine();
        p.BeginOutputReadLine();
        using var reg = ct.Register(() => p.Kill(true));
        await p.WaitForExitAsync(ct);

        return p.ExitCode;
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        return Directory.EnumerateFiles(JavaHome, "*", SearchOption.AllDirectories)
            .Select(f => new FileEntry(f)
                .WithArchiveEntryName(DistName, Path.GetRelativePath(JavaHome, f)));
    }

    public void Dispose()
    {
        _storage.Dispose();
    }
}
