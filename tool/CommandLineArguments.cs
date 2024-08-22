using Cascadium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cascadiumtool;

internal class CommandLineArguments
{
    internal bool IsCompiled = false;
    internal List<Regex> CompiledExcludes = new List<Regex>();

    public List<string> InputFiles { get; set; } = new();
    public List<string> InputDirectories { get; set; } = new();
    public List<string> Extensions { get; set; } = new List<string>();
    public List<string> Exclude { get; set; } = new List<string>();
    public bool StdIn { get; set; } = false;
    public string? ConfigFile { get; set; }
    public string? OutputFile { get; set; }
    public bool Pretty { get; set; } = true;
    public bool UseVarShortcuts { get; set; } = true;
    public bool KeepNestingSpace { get; set; } = false;
    public string? MergeOption { get; set; }
    public string? MergeOrder { get; set; }
    public FilenameTagOption? FilenameTag { get; set; }
    public bool Watch { get; set; } = false;

    public void Import(JsonCssCompilerOptions? jsonConfig)
    {
        if (!IsCompiled)
        {
            InputDirectories = new List<string>(InputDirectories);
            InputFiles = new List<string>(InputFiles);
            Extensions = new List<string>(Extensions);
            Exclude = new List<string>(Exclude);

            if (jsonConfig != null)
            {
                foreach (var file in jsonConfig.InputFiles) InputFiles.Add(file);
                foreach (var dir in jsonConfig.InputDirectories) InputDirectories.Add(dir);
                foreach (var ext in jsonConfig.Extensions) Extensions.Add(ext);
                foreach (var exc in jsonConfig.ExcludePatterns) Exclude.Add(exc);
                if (jsonConfig.OutputFile != null) OutputFile = jsonConfig.OutputFile;
            }

            foreach (string exPattern in Exclude)
            {
                try
                {
                    CompiledExcludes.Add(new Regex(exPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                }
                catch (Exception ex)
                {
                    Log.ErrorKill("Couldn't parse the exclude regex " + exPattern + ": " + ex.Message);
                }
            }

            IsCompiled = true;
        }
    }
}