using CurseTheBeast.Api.FTB;
using Spectre.Console;

namespace CurseTheBeast.Utils;


public static class ErrorUtils
{
    const string IndentPrefix = "  ";
    const int ExceptionMaxLines = 5;

    static readonly IReadOnlySet<Type> IgnoredInnerExceptionTypes = new HashSet<Type>() 
    {
        typeof(OperationCanceledException),
        typeof(TimeoutException),
        typeof(HttpRequestException),
    };
    static readonly IReadOnlySet<Type> SimpleExceptionTypes = new HashSet<Type>()
    {
        typeof(FTBException),
        typeof(Exception),
    };

    public static void Handler(Exception ex)
    {
        AnsiConsole.WriteLine();
        if (ex is Spectre.Console.Cli.CommandAppException cliEx)
        {
            Error.WriteLine(cliEx.Message);
            return;
        }
        Handler(ex, false);
        AnsiConsole.WriteLine();
        Error.WriteLine("发生了错误");
    }

    static void Handler(Exception ex, bool inner = false)
    {
        if (SimpleExceptionTypes.Contains(ex.GetType()))
        {
            if (inner)
                Error.WriteLine(Indent(ex.Message));
            else
                Error.WriteLine("× " + ex.Message);
            if (ex.InnerException != null)
                Handler(ex.InnerException, true);
        }
        else if (!inner || !IgnoredInnerExceptionTypes.Any(t => inner.GetType().IsAssignableTo(t)))
        {
            if (inner)
                Error.WriteLine(Indent(getString(ex)));
            else
                Error.WriteLine(getString(ex));
        }
    }

    static string getString(Exception ex)
    {
        return string.Join(Environment.NewLine, ex.ToString()
            .Replace("\r\n", "\n")
            .Split("\n")
            .Take(ExceptionMaxLines));
    }

    static string Indent(string str)
    {
        return IndentPrefix + str.ReplaceLineEndings(IndentPrefix + Environment.NewLine);
    }
}
