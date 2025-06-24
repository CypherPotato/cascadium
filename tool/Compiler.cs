using Cascadium;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace cascadiumtool;

internal class Compiler
{
    static readonly Regex VendorRegex = new Regex(@"[\\/]vendor[\\/]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static async Task<int> RunCompiler(CascadiumCompilationConfiguration args)
    {
        if (args.OutputFile == null)
            Log.LoggingEnabled = false;

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
            Pretty = args.Pretty,
            UseVarShortcut = args.UseVarShortcuts,
            KeepNestingSpace = args.KeepNestingSpace,
            AtRulesRewrites = args.AtRuleRewriters.Aggregate(new NameValueCollection(),
                (seed, current) =>
                {
                    seed.Add(current.Key, current.Value);
                    return seed;
                })
        };

        options.Converters.AddRange(args.Converters);

        if (!string.IsNullOrEmpty(stdin))
        {
            anyCompiled = true;
            string result = Cascadium.CascadiumCompiler.Compile(stdin, options);
            Console.Out.Write(result);
        }

        {
            List<string> includedExtensions = [".xcss"];
            List<string> inputFiles = [];

            string? outputFile = null;
            if (!string.IsNullOrEmpty(args.OutputFile))
            {
                outputFile = PathUtils.ResolvePath(args.OutputFile);
                PathUtils.EnsureExistence(Path.GetDirectoryName(outputFile)!);
            }

            if (args.Extensions.Count > 0)
                includedExtensions.AddRange(args.Extensions);

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
            foreach (Regex exRegex in args.Exclude)
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
                ConcurrentDictionary<string, string> resultCss = new ConcurrentDictionary<string, string>();

                CancellationTokenSource errorCanceller = new CancellationTokenSource();

                try
                {
                    await Parallel.ForEachAsync(inputFiles,
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = Environment.ProcessorCount * 4,
                            CancellationToken = errorCanceller.Token
                        }, (file, ct) =>
                        {
                            if (ct.IsCancellationRequested)
                                return ValueTask.FromCanceled(ct);

                            string fileRelativeName = file.Substring(smallInputLength + 1);

                            try
                            {
                                string result = CompileFile(file, options);
                                Interlocked.Add(ref compiledLength, result.Length);

                                if (args.FilenameTag is FilenameTagOption.Full)
                                {
                                    result = CommentString(file) + (options.Pretty ? Environment.NewLine : string.Empty) + result;
                                }
                                else if (args.FilenameTag is FilenameTagOption.Relative)
                                {
                                    result = CommentString(fileRelativeName) + (options.Pretty ? Environment.NewLine : string.Empty) + result;
                                }

                                resultCss.TryAdd(file, result);
                            }
                            catch (CascadiumException cex)
                            {
                                string linePadText = cex.Line + ".";
                                AnsiConsole.MarkupLine($"[grey]cascadium[/] [silver]{DateTime.Now:T}[/] [indianred_1]error[/] at file [white]{fileRelativeName}[/], line [deepskyblue3_1]{cex.Line}[/], col. [deepskyblue3_1]{cex.Column}[/]:\n");
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

                if (errorCanceller.IsCancellationRequested)
                {
                    return 2;
                }

                StringBuilder resultCssBuilder = new StringBuilder();

                var resultsOrdered = resultCss
                    .OrderBy(k => VendorRegex.IsMatch(k.Key) ? string.Empty : Path.GetDirectoryName(k.Key));

                foreach (KeyValuePair<string, string> item in resultsOrdered)
                {
                    resultCssBuilder.Append(item.Value);
                    if (options.Pretty)
                        resultCssBuilder.AppendLine();
                }

                string css = resultCssBuilder.ToString();
                if (outputFile != null)
                {
                    File.WriteAllText(outputFile, css);

                    compiledLength = new FileInfo(outputFile).Length;
                    if (!args.Watch)
                        Log.Info($"{inputFiles.Count} file(s) -> {Path.GetFileName(args.OutputFile)} ({PathUtils.FileSize(compiledLength)}) in {sw.ElapsedMilliseconds:N0}ms");
                }
                else
                {
                    Console.Write(css);
                }

                if (args.MergeOption is { } m && m != MergeOption.None)
                {
                    if (args.Watch)
                    {
                        if (Log.MergeOnWatchNotify == false)
                        {
                            Log.Warn("Ignoring CSS merge due to watch");
                            Log.MergeOnWatchNotify = true;
                        }
                    }
                    else
                    {
                        Log.Info("Merging...");
                        options.Merge = m;
                        options.MergeOrderPriority = args.MergeOrder;

                        string mergedCss = CascadiumCompiler.Compile(css, options);
                        if (outputFile != null)
                        {
                            File.WriteAllText(outputFile, mergedCss);
                            compiledLength = new FileInfo(outputFile).Length;
                            Log.Info($"Output merged. New file size: {PathUtils.FileSize(compiledLength)}");
                        }
                        else
                        {
                            Console.Write(mergedCss);
                        }
                    }
                }
            }
        }

        if (!anyCompiled)
        {
            return Log.Warn("no file or input was compiled.");
        }

        return 0;
    }

    static string CommentString(string text) => $"/* {text.Replace("*", "")} */";

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
