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
            KeepNestingSpace = args.KeepNestingSpace == BoolType.True,
            Merge = args.Merge,
            MergeOrderPriority = args.MergeOrderPriority
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

                string rawCss = string.Join("\n", inputFiles.Select(File.ReadAllText));

                long compiledLength = 0, totalLength = rawCss.Length;
                string result = Cascadium.CascadiumCompiler.Compile(rawCss, options);

                if (outputFile != null)
                {
                    File.WriteAllText(outputFile, result);
                    compiledLength = new FileInfo(outputFile).Length;
                    Log.Info($"{inputFiles.Count} file(s) -> {Path.GetFileName(args.OutputFile)} [{PathUtils.FileSize(totalLength)} -> {PathUtils.FileSize(compiledLength)}]");
                }
                else
                {
                    Console.Write(result.ToString());
                }
            }
        }

        if (!anyCompiled)
        {
            return Log.ErrorKill("no file or input was compiled.");
        }

        return 0;
    }
}
