using System.Text.Json.Serialization;

namespace CurseTheBeast.Api.Mojang.Model;


public partial class GameManifest
{
    public Downloads downloads { get; init; } = null!;
    public JavaVersion javaVersion { get; init; } = null!;

    public class Downloads
    {
        public Server server { get; init; } = null!;

        public class Server
        {
            public string sha1 { get; init; } = null!;
            public int size { get; init; }
            public string url { get; init; } = null!;
        }
    }

    public class JavaVersion
    {
        public string component { get; init; } = null!;
        public int majorVersion { get; init; }
    }

    [JsonSerializable(typeof(GameManifest))]
    public partial class GameManifestContext : JsonSerializerContext
    {

    }
}












