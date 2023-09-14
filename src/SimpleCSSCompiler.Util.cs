using System.Text;

namespace SimpleCSS;
public sealed partial class SimpleCSSCompiler
{
    private static void TrimEndSb(StringBuilder sb)
    {
        int i = sb.Length - 1;

        for (; i >= 0; i--)
            if (!char.IsWhiteSpace(sb[i]))
                break;

        if (i < sb.Length - 1)
            sb.Length = i + 1;
    }

    private static bool IsNameChar(char c)
    {
        return Char.IsLetter(c) || Char.IsDigit(c) || c == '_' || c == '-';
    }
}
