namespace CurseTheBeast.Api.Mojang.Model;


public class GameVersions
{
    public Version[] versions { get; init; } = null!;

    public class Version
    {
        public string id { get; init; } = null!;
        public string url { get; init; } = null!;
    }
}
