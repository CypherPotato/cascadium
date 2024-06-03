using Cascadium.Compiler;
using Cascadium.Entity;

namespace Cascadium.Extensions;

internal class MediaRewriter
{
    public static void ApplyRewrites(CssStylesheet cssStylesheet, CascadiumOptions options)
    {
        foreach (var subcss in cssStylesheet._stylesheets)
        {
            if (subcss.AtRuleDeclaration == null) continue;
            foreach (string rewrite in options.AtRulesRewrites)
            {
                if (Helper.InvariantCompare(subcss.AtRuleDeclaration?.TrimStart('@'), rewrite.TrimStart('@')))
                {
                    subcss.AtRuleDeclaration = '@' + options.AtRulesRewrites[rewrite]!.TrimStart('@');
                }
            }
        }
    }
}
