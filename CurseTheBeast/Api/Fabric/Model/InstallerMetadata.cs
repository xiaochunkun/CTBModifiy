namespace CurseTheBeast.Api.Fabric.Model;


public class InstallerMetadata
{
    public string url { get; init; } = null!;
    public string version { get; init; } = null!;
    public bool stable { get; init; }
}
