using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;

internal abstract class CompilerModule
{
    public CompilerContext Context { get; private set; }

    public CascadiumOptions? Options { get => Context.Options; }

    public CompilerModule(CompilerContext context)
    {
        this.Context = context;
    }
}
