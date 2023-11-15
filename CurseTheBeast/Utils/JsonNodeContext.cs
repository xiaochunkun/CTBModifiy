using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CurseTheBeast.Utils;

[JsonSerializable(typeof(JsonNode))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class JsonNodeContext : JsonSerializerContext
{

}
