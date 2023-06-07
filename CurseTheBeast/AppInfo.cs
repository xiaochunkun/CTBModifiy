using System.Reflection;

namespace CurseTheBeast;


public static class AppInfo
{
    public static readonly string Name;
    public static readonly string Version;
    public static readonly string Author;

    static AppInfo()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        Name = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "CurseTheBeast";
        Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0";
        Author = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "TomatoPuddin";
    }
}
