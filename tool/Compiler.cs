using Cascadium;
using Microsoft.VisualBasic;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace cascadiumtool;

internal class Compiler
{
    public static async Task<int> RunCompiler(CommandLineArguments args)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        string? stdin = null;
        bool anyCompiled = false;

        if (Console.IsInputRedirected && args.StdIn)
        {
            using (var reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
            {
                stdin = reader.ReadToEnd();
            }
        }

        CascadiumOptions options = new Cascadium.CascadiumOptions()
        {
            Pretty = args.Pretty == BoolType.True,
            UseVarShortcut = args.UseVarShortcuts == BoolType.True,
            KeepNestingSpace = args.KeepNestingSpace == BoolType.True
        };

        Program.CompilerOptions?.ApplyConfiguration(options);

        if (!string.IsNullOrEmpty(stdin))
        {
            anyCompiled = true;
            string result = Cascadium.CascadiumCompiler.Compile(stdin, options);
            Console.Out.Write(result);
        }

        {
            List<string> includedExtensions = new List<string>() { ".xcss" };
            List<string> inputFiles = new List<string>();

            string? outputFile = null;
            if (!string.IsNullOrEmpty(args.OutputFile))
            {
                outputFile = PathUtils.ResolvePath(args.OutputFile);
                PathUtils.EnsureExistence(Path.GetDirectoryName(outputFile)!);
            }

            if (args.Extensions.Count > 0)
                includedExtensions.AddRange(args.Extensions.Cast<string>());

            foreach (string f in args.InputFiles)
            {
                string fullPath = PathUtils.ResolvePath(f);

                if (!File.Exists(fullPath))
                    return Log.ErrorKill($"the specified input file \"{f}\" was not found.");

                if (string.Compare(fullPath, outputFile, true) == 0)
                    continue;

                if (!inputFiles.Contains(fullPath))
                    inputFiles.Add(fullPath);
            }

            foreach (string d in args.InputDirectories)
            {
                string fullPath = PathUtils.ResolvePath(d);

                if (!Directory.Exists(fullPath))
                    return Log.ErrorKill($"the specified input directory \"{d}\" was not found.");

                string[] allFiles = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories)
                    .Where(df => includedExtensions.Contains(Path.GetExtension(df)))
                    .OrderBy(d => d.Count(c => c == Path.DirectorySeparatorChar))
                    .ToArray();

                inputFiles.AddRange(allFiles
                    .Where(f => string.Compare(f, outputFile, true) != 0 && !inputFiles.Contains(f)));
            }

            // apply exclude patterns
            foreach (Regex exRegex in args.CompiledExcludes)
            {
                inputFiles = inputFiles
                    .Where(i => !exRegex.IsMatch(i))
                    .ToList();
            }

            if (inputFiles.Count > 0)
            {
                anyCompiled = true;
                long compiledLength = 0;
                int smallInputLength = inputFiles.Select(Path.GetDirectoryName).Min(i => i?.Length ?? 0);
                ConcurrentBag<string> resultCss = new ConcurrentBag<string>();

                CancellationTokenSource errorCanceller = new CancellationTokenSource();

                try
                {
                    await Parallel.ForEachAsync(inputFiles,
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = 4,
                            CancellationToken = errorCanceller.Token
                        }, (file, ct) =>
                        {
                            if (ct.IsCancellationRequested)
                                return ValueTask.FromCanceled(ct);

                            try
                            {
                                string result = CompileFile(file, options);
                                Interlocked.Add(ref compiledLength, result.Length);
                                resultCss.Add(result);
                            }
                            catch (CascadiumException cex)
                            {
                                string linePadText = cex.Line + ".";
                                AnsiConsole.MarkupLine($"[grey]cascadium[/] [silver]{DateTime.Now:T}[/] [indianred_1]error[/] at file [white]{file.Substring(smallInputLength + 1)}[/], line [deepskyblue3_1]{cex.Line}[/], col. [deepskyblue3_1]{cex.Column}[/]:\n");
                                AnsiConsole.MarkupInterpolated($"\t[deepskyblue3_1]{linePadText}[/] [silver]{cex.LineText.TrimEnd()}[/]\n");
                                AnsiConsole.MarkupLine($"\t[lightpink4]{new string(' ', cex.Column + linePadText.Length)}^[/]");
                                AnsiConsole.MarkupInterpolated($"\t[mistyrose3]{cex.Message}[/]");
                                AnsiConsole.WriteLine();
                                errorCanceller.Cancel();
                            }

                            return ValueTask.CompletedTask;
                        });
                }
                catch (TaskCanceledException)
                {
                    ;
                }

                string css = string.Join(options.Pretty ? "\n" : "", resultCss);
                if (outputFile != null)
                {         
                    File.WriteAllText(outputFile, css);

                    compiledLength = new FileInfo(outputFile).Length;
                    if (!Program.IsWatch)
                        Log.Info($"{inputFiles.Count} file(s) -> {Path.GetFileName(args.OutputFile)} ({PathUtils.FileSize(compiledLength)}) in {sw.ElapsedMilliseconds:N0}ms");
                }
                else
                {
                    Console.Write(css);
                }
            }
        }

        if (!anyCompiled)
        {
            return Log.ErrorKill("no file or input was compiled.");
        }

        return 0;
    }

    static string CompileFile(string file, CascadiumOptions options)
    {
        string result;

        lock (Program.CompilerCache)
            if (!Program.CompilerCache.TryGetValue(file, out result!))
            {
                result = CascadiumCompiler.Compile(File.ReadAllText(file), options);
                Program.CompilerCache.Add(file, result);
            }

        return result;
    }
}
