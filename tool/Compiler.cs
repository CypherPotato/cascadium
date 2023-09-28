using SimpleCSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tool;

internal class Compiler
{
    public static int RunCompiler(CommandLineArguments args)
    {
        string? stdin = null;
        bool anyCompiled = false;

        if (Console.IsInputRedirected)
        {
            using (var reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
            {
                stdin = reader.ReadToEnd();
            }
        }

        CSSCompilerOptions options = new SimpleCSS.CSSCompilerOptions()
        {
            Pretty = args.Pretty == BoolType.True,
            UseVarShortcut = args.UseVarShortcuts == BoolType.True,
            KeepNestingSpace = args.KeepNestingSpace == BoolType.True,
        };

        if (!string.IsNullOrEmpty(stdin))
        {
            anyCompiled = true;
            string result = SimpleCSSCompiler.Compile(stdin, options);
            Console.Out.Write(result);
        }

        {
            List<string> includedExtensions = new List<string>() { ".xcss" };
            List<string> inputFiles = new List<string>();

            if (args.Extensions.Any())
                includedExtensions.AddRange(args.Extensions);

            foreach (string f in args.InputFiles)
            {
                string fullPath = PathUtils.ResolvePath(f);

                if (!File.Exists(fullPath))
                    return Log.ErrorKill($"the specified input file \"{f}\" was not found.");

                inputFiles.Add(fullPath);
            }

            foreach (string d in args.InputDirectories)
            {
                string fullPath = PathUtils.ResolvePath(d);

                if (!Directory.Exists(fullPath))
                    return Log.ErrorKill($"the specified input directory \"{d}\" was not found.");

                string[] allFiles = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories)
                    .Where(df => includedExtensions.Contains(Path.GetExtension(df)))
                    .ToArray();

                inputFiles.AddRange(allFiles);
            }

            if (inputFiles.Count > 0)
            {
                anyCompiled = true;

                string? outputFile = null;
                if (!string.IsNullOrEmpty(args.OutputFile))
                {
                    outputFile = PathUtils.ResolvePath(args.OutputFile);
                    PathUtils.EnsureExistence(Path.GetDirectoryName(outputFile)!);
                }

                StringBuilder compiled = new StringBuilder();
                foreach (string f in inputFiles)
                {
                    string contents = File.ReadAllText(f);
                    string result = SimpleCSSCompiler.Compile(contents, options);
                    compiled.Append(result);
                }

                if (outputFile != null)
                {
                    File.WriteAllText(outputFile, compiled.ToString());
                    Log.Info($"{inputFiles.Count} file(s) -> {Path.GetFileName(args.OutputFile)} [{PathUtils.FileSize(compiled.Length)}]");
                }
                else
                {
                    Console.Write(compiled.ToString());
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
