namespace CurseTheBeast.Mirrors;


public class McbbsMirror : HostReplacementMirror
{
    public static readonly McbbsMirror Instance = new();

    public McbbsMirror() : base(new Dictionary<IReadOnlyList<string>, string>()
    {
        [new[]
        {
            "launcher.mojang.com",
            "launchermeta.mojang.com",
            "piston-meta.mojang.com",
            "piston-data.mojang.com",
            "files.minecraftforge.net",
        }] = "download.mcbbs.net",
        [new[]
        {
            "libraries.minecraft.net",
            "maven.minecraftforge.net",
            "maven.fabricmc.net",
        }] = "download.mcbbs.net/maven",
        /* 缺的太多
        [new[]
        {
            "meta.fabricmc.net",
        }] = "download.mcbbs.net/fabric-meta",
        */
    })
    {

    }
}
