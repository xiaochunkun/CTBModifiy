using System.Text.Json.Serialization;

namespace CurseTheBeast.Services.Model;


public partial class ModpackCache
{
    public Dictionary<int, Item> Items { get; set; } = new Dictionary<int, Item>();

    public class Item
    {
        public string Name { get; set; } = null!;
        public int Update { get; set; }
    }

    [JsonSerializable(typeof(ModpackCache))]
    public partial class ModpackCacheContext : JsonSerializerContext
    {

    }
}
