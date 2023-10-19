using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;
internal class Utils : CompilerModule
{
    public Utils(CompilerContext context) : base(context)
    {
    }

    public void TrimEndSb(StringBuilder sb)
    {
        int i = sb.Length - 1;

        for (; i >= 0; i--)
            if (!char.IsWhiteSpace(sb[i]))
                break;

        if (i < sb.Length - 1)
            sb.Length = i + 1;
    }

    public bool IsNameChar(char c)
        => Char.IsLetter(c) || Char.IsDigit(c) || c == '_' || c == '-';

    public string RemoveSpaces(string s)
        => new String(s.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
}
