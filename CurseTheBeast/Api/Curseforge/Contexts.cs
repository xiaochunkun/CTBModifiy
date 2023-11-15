using CurseTheBeast.Api.Curseforge.Model;
using System.Text.Json.Serialization;

namespace CurseTheBeast.Api.Curseforge;

public partial class Contexts
{
    [JsonSerializable(typeof(GenericRsp<ModFile[]>))]
    public partial class ModFileArrayContext : JsonSerializerContext
    {

    }
}
