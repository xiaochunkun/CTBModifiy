using System.Security.Cryptography;

namespace CurseTheBeast.Storage;


public class FileEntry
{
    public string LocalPath { get; private init; }
    public string LocalTempPath { get; private init; }
    public string? DisplayName { get; private set; }
    public string? ArchiveEntryName { get; private set; }
    public string? Url { get; private set; }
    public bool Required { get; private set; } = true;
    public bool UnixExecutable { get; private set; } = false;
    public bool Unreachable { get; private set; } = false;
    public string? Sha1 { get; private set; }
    public int? Size { get; private set; }

    string _sha1FilePath;
    bool _validated = false;

    public FileEntry(RepoType repo, params string[] path)
        : this(LocalStorage.Persistent, repo, path)
    {

    }

    public FileEntry(RepoType repo, string path)
        : this(LocalStorage.Persistent, repo, path)
    {

    }

    public FileEntry(LocalStorage storage, RepoType repo, params string[] pathSegments)
        : this(storage.GetFilePath(repo, pathSegments))
    {

    }

    public FileEntry(LocalStorage storage, RepoType repo, string path)
        : this(storage.GetFilePath(repo, path))
    {

    }

    public FileEntry(string localPath)
    {
        LocalPath = localPath;
        LocalTempPath = localPath + ".tmp";
        _sha1FilePath = localPath + ".sha1";
    }

    public FileEntry SetUnixExecutable()
    {
        UnixExecutable = true;
        return this;
    }

    public FileEntry SetUnreachable()
    {
        Unreachable = true;
        return this;
    }

    public FileEntry SetUnrequired()
    {
        Required = false;
        return this;
    }

    public FileEntry WithSha1(string sha1)
    {
        Sha1 = sha1;
        return this;
    }

    public FileEntry WithSize(int size)
    {
        Size = size;
        return this;
    }

    public FileEntry WithArchiveEntryName(IEnumerable<string?> entryName)
    {
        ArchiveEntryName = string.Join('/', entryName
            .Where(entryName => !string.IsNullOrWhiteSpace(entryName))
            .Select(entryName => entryName!
                .Replace(Path.DirectorySeparatorChar, '/')
                .TrimStart('.')
                .Trim('/')));
        return this;
    }

    public FileEntry WithArchiveEntryName(params string?[] entryName)
    {
        return WithArchiveEntryName(entryName as IEnumerable<string?>);
    }

    public FileEntry SetDownloadable(string displayName, string url)
    {
        DisplayName = displayName;
        Url = url;
        return this;
    }

    public bool ValidateTemp()
    {
        return validateOrDel(new FileInfo(LocalTempPath));
    }

    public bool Validate()
    {
        return _validated = _validated || validateOrDel(new FileInfo(LocalPath));
    }

    public void ApplyTemp()
    {
        File.Move(LocalTempPath, LocalPath, true);
    }

    public void Delete()
    {
        if (File.Exists(LocalPath))
            File.Delete(LocalPath);
    }

    public void DeleteTemp()
    {
        if (File.Exists(LocalTempPath))
            File.Delete(LocalTempPath);
    }

    bool validateOrDel(FileInfo file)
    {
        if (!file.Exists)
            return false;

        // 如果已提供文件大小且不匹配，直接失败
        if (Size != null && file.Length != Size)
        {
            file.Delete();
            return false;
        }

        byte[]? provided = null;
        if (Sha1 == null)
        {
            // 如果未提供Sha1，尝试从文件读取
            if (File.Exists(_sha1FilePath))
                provided = File.ReadAllBytes(_sha1FilePath);
        }
        else
        {
            // 如果已提供，直接读取
            provided = Convert.FromHexString(Sha1);
        }

        using var fs = file.OpenRead();
        var computed = SHA1.HashData(fs);
        if (provided != null)
        {
            // 如果对比失败，删除文件和哈希文件
            if (!computed.SequenceEqual(provided))
            {
                fs.Close();
                file.Delete();
                if (File.Exists(_sha1FilePath))
                    File.Delete(_sha1FilePath);
                return false;
            }
        }
        else
        {
            // 如果未读取到哈希，视为成功，并写入哈希
            File.WriteAllBytes(_sha1FilePath, computed);
        }

        return true;
    }
}
