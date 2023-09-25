using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tool;

internal static class Watcher
{
    private static MemoryCache watcherCache = new MemoryCache("xcss-mem-cache");
    private static FileSystemWatcher fsWatcher = new FileSystemWatcher();
    private static CommandLineArguments watchArgs = null!;
    private static string[] watchingDirectories = Array.Empty<string>();    

    public static int Watch(CommandLineArguments args)
    {
        watchArgs = args;
        HashSet<string> paths = new HashSet<string>();

        foreach (string fdir in args.InputDirectories)
        {
            paths.Add(PathUtils.ResolvePath(fdir));
        }
        foreach (string sfdir in args.InputFiles)
        {
            string fdir = Path.GetDirectoryName(PathUtils.ResolvePath(sfdir))!;
            paths.Add(fdir);
        }

        string? smallestPath = null;
        if (paths.Count == 0)
        {
            Log.Info("No directory was included to watch. Using the current directory instead.");
            paths.Add(Program.CurrentDirectory);
            args.InputDirectories = new string[1] { Program.CurrentDirectory };
        }

        watchingDirectories = paths.ToArray();
        foreach (string p in paths)
        {
            if (smallestPath == null || p.Length < smallestPath.Length)
            {
                smallestPath = p;
            }
        }

        fsWatcher.Path = smallestPath!;
        fsWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;
        fsWatcher.Changed += FsWatcher_Changed;
        fsWatcher.Filter = "*.*";
        fsWatcher.IncludeSubdirectories = true;
        fsWatcher.EnableRaisingEvents = true;

        Log.Info("xcss is watching for file changes");
        Log.LoggingEnabled = false;

        Thread.Sleep(-1);
        return 0;
    }

    private static void FsWatcher_Changed(Object sender, FileSystemEventArgs e)
    {
        if (watcherCache.Get(e.FullPath) != null) return;
        string file = e.FullPath;
        string ext = Path.GetExtension(file);
        if (ext != ".xcss" && !watchArgs.Extensions.Contains(ext))
        {
            return;
        }
        bool isDirIncluded = false;
        foreach (string includedDir in watchingDirectories)
            isDirIncluded |= file.StartsWith(includedDir);
        if (!isDirIncluded)
            return;

        watcherCache.Add(e.FullPath, true, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMilliseconds(500) });

        try
        {
            Compiler.RunCompiler(watchArgs);

        } catch (System.IO.IOException)
        {
            // The process cannot access the file <name> because it is being used by another process.
            return;
        }
    }
}
