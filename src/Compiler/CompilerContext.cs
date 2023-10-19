using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;

internal class CompilerContext
{
    public string? AtRule { get; set; }
    public List<CssRule> Rules { get; set; } = new List<CssRule>();
    public List<CompilerContext> Stylesheets { get; set; } = new List<CompilerContext>();
    public List<string> Declarations { get; set; } = new List<string>();
    public int StackOrder { get; set; } = 0;
    public CascadiumOptions? Options { get; set; }

    public readonly Merger Merger;
    public readonly Utils Utils;
    public readonly Exporter Exporter;
    public readonly Parser Parser;
    public readonly Preparers Preparers;
    public readonly Split Split;

    public CompilerContext()
    {
        Merger = new Merger(this);
        Utils = new Utils(this);
        Exporter = new Exporter(this);
        Parser = new Parser(this);
        Preparers = new Preparers(this);
        Split = new Split(this);
    }
}
