using Cascadium;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cascadiumtool;

internal class Compiler
{
    public static int RunCompiler(CommandLineArguments args)
    {
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
                StringBuilder resultCss = new StringBuilder();

                long compiledLength = 0, totalLength = 0;
                int smallInputLength = inputFiles.Select(Path.GetDirectoryName).Min(i => i?.Length ?? 0);

                foreach (string file in inputFiles)
                {
                    string contents = ReadFile(file);
                    string result;
                    totalLength += contents.Length;

                    try
                    {
                        result = CascadiumCompiler.Compile(contents, options);
                        compiledLength += result.Length;

                        if (options.Pretty)
                        {
                            resultCss.AppendLine(result + "\n");
                        }
                        else
                        {
                            resultCss.Append(result);
                        }
                    }
                    catch (CascadiumException cex)
                    {
                        Console.WriteLine($"error at file {file.Substring(smallInputLength + 1)}, line {cex.Line}, col. {cex.Column}:");
                        Console.WriteLine();
                        Console.WriteLine($"\t{cex.LineText}");
                        Console.WriteLine($"\t{new string(' ', cex.Column - 1)}^");
                        Console.WriteLine($"\t{cex.Message}");
                        return 5;
                    }
                }

                if (outputFile != null)
                {
                    File.WriteAllText(outputFile, resultCss.ToString());
                    compiledLength = new FileInfo(outputFile).Length;
                    Log.Info($"{inputFiles.Count} file(s) -> {Path.GetFileName(args.OutputFile)} [{PathUtils.FileSize(totalLength)} -> {PathUtils.FileSize(compiledLength)}]");
                }
                else
                {
                    Console.Write(resultCss.ToString());
                }
            }
        }

        if (!anyCompiled)
        {
            return Log.ErrorKill("no file or input was compiled.");
        }

        return 0;
    }

    static string ReadFile(string file)
    {
        return System.IO.File.ReadAllText(file);
    }
}
