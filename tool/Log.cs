using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cascadiumtool;
internal static class Log
{
    public static bool LoggingEnabled { get; set; } = true;

    public static int ErrorKill(string message)
    {
        Write("error", message);
        return 1;
    }

    public static int Info(string message, bool force = false)
    {
        if (!force && !LoggingEnabled) return 0;
        Write("info", message);
        return 0;
    }

    private static void Write(string level, string message)
    {
        if (level == "error")
        {
            AnsiConsole.MarkupLine($"[grey]cascadium[/] [silver]{DateTime.Now:T}[/] [indianred_1]error[/] [white]{message}[/]");
        }
        else if (level == "info")
        {
            AnsiConsole.MarkupLine($"[grey]cascadium[/] [silver]{DateTime.Now:T}[/] [darkcyan]info[/] [white]{message}[/]");
        }
        else if (level == "warn")
        {
            AnsiConsole.MarkupLine($"[grey]cascadium[/] [silver]{DateTime.Now:T}[/] [darkgoldenrod]info[/] [white]{message}[/]");
        }
    }
}
