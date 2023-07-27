using Spectre.Console;

namespace CurseTheBeast.Utils;


public class EnvironmentUtils
{
    public static void CheckTerminal()
    {
        if (AnsiConsole.Profile.Capabilities.Ansi && AnsiConsole.Profile.Capabilities.Interactive)
            return;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            if (NativeUtils.IsRunningByDoubleClick.Value)
                throw new Exception("当前操作系统不支持双击启动本程序，请升级至 win10 1607 或更高版本，或通过命令行启动");
            else
                throw new Exception("当前终端不支持无参启动，请指定具体的命令行参数，或升级操作系统至 win10 1607 或更高版本");
        }
        else
        {
            throw new Exception("当前终端不支持无参启动，请指定具体的命令行参数，或换用其它终端");
        }
    }
}
