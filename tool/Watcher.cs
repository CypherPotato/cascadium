using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace cascadiumtool;

internal static class Watcher
{
    private static readonly FileSystemWatcher fsWatcher = new FileSystemWatcher();
    private static CommandLineArguments watchArgs = null!;
    private static string[] watchingDirectories = Array.Empty<string>();
    private static bool IsRunningCompilation = false;

    public static async Task<int> Watch(CommandLineArguments args)
    {
        watchArgs = args;
        HashSet<string> paths = new HashSet<string>();

        if (args.OutputFile is null)
        {
            return Log.ErrorKill("Seems like you're trying to run watch without specifying an output file.");
        }

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
            paths.Add(Program.CurrentDirectory);
            args.InputDirectories = new() { Program.CurrentDirectory };
        }

        watchingDirectories = paths.ToArray();
        foreach (string p in paths)
        {
            if (!Directory.Exists(p))
            {
                return Log.ErrorKill("The detected directory path at " + p + " does not exists.");
            }

            if (smallestPath == null || p.Length < smallestPath.Length)
            {
                smallestPath = p;
            }
        }

        fsWatcher.Path = smallestPath!;
        fsWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;

        fsWatcher.Changed += FsWatcher_Changed;
        fsWatcher.Renamed += FsWatcher_Changed;
        fsWatcher.Deleted += FsWatcher_Changed;
        fsWatcher.Created += FsWatcher_Changed;

        fsWatcher.Filter = "*.*";
        fsWatcher.IncludeSubdirectories = true;
        fsWatcher.EnableRaisingEvents = true;

        await Compiler.RunCompiler(args);

        Log.Info("");
        Log.Info("Ready! Cascadium is now watching for file changes.");
        //Log.LoggingEnabled = false;

        Thread.Sleep(-1);
        return 0;
    }

    private static async void FsWatcher_Changed(Object sender, FileSystemEventArgs e)
    {
        try
        {
            if (IsRunningCompilation)
            {
                return;
            }

            IsRunningCompilation = true;

            string outFile = PathUtils.ResolvePath(watchArgs.OutputFile!);
            if (outFile == e.FullPath)
            {
                // avoid compiling the out file
                return;
            }

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

            Program.CompilerCache.Remove(file);

            if (e.ChangeType == WatcherChangeTypes.Renamed
             || e.ChangeType == WatcherChangeTypes.Deleted
             || e.ChangeType == WatcherChangeTypes.Created)
            {
                Log.Info($"Directory structure modified. Clearing cache.");
                Program.CompilerCache.Clear();
            }

            await Task.Delay(100); // prevent the below error giving time to the time to write
            Log.Info($"Detected {e.ChangeType} on {Path.GetFileName(file)}, building...");

            await Compiler.RunCompiler(watchArgs);
            ;
        }
        catch (System.IO.IOException)
        {
            // The process cannot access the file <name> because it is being used by another process.
            return;
        }
        finally
        {
            await Task.Delay(400);
            IsRunningCompilation = false;
        }
    }
}
