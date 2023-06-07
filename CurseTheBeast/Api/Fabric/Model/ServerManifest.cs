namespace CurseTheBeast.Api.Fabric.Model;


public class ServerManifest
{
    public string mainClass { get; init; } = null!;
    public Library[] libraries { get; init; } = null!;


    public class Library
    {
        public string name { get; init; } = null!;
        public string url { get; init; } = null!;
    }
}
