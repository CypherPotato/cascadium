using System;

namespace Cascadium.Compiler;
static class AtRule
{
    public static bool IsNotEligibleToSelectorMerge(string selector)
        =>
        selector.StartsWith("@font-face", StringComparison.InvariantCultureIgnoreCase) ||
        selector.StartsWith("@counter-style", StringComparison.InvariantCultureIgnoreCase) ||
        selector.StartsWith("@color-profile", StringComparison.InvariantCultureIgnoreCase) ||
        selector.StartsWith("@property", StringComparison.InvariantCultureIgnoreCase)
        ;

    public static bool IsGroupAtRule(string atRule)
        =>
        atRule.StartsWith("@media", StringComparison.InvariantCultureIgnoreCase) ||
        atRule.StartsWith("@scope", StringComparison.InvariantCultureIgnoreCase) ||
        atRule.StartsWith("@supports", StringComparison.InvariantCultureIgnoreCase) ||
        atRule.StartsWith("@page", StringComparison.InvariantCultureIgnoreCase) ||
        atRule.StartsWith("@keyframes", StringComparison.InvariantCultureIgnoreCase) ||
        atRule.StartsWith("@counter-style", StringComparison.InvariantCultureIgnoreCase) ||
        atRule.StartsWith("@layer", StringComparison.InvariantCultureIgnoreCase) ||
        atRule.StartsWith("@container", StringComparison.InvariantCultureIgnoreCase)
        ;

    public static bool IsNotParentInherited(string selector)
        =>
        selector.StartsWith("@keyframes", StringComparison.InvariantCultureIgnoreCase) ||
        selector.StartsWith("@page", StringComparison.InvariantCultureIgnoreCase) ||
        selector.StartsWith("@property", StringComparison.InvariantCultureIgnoreCase) ||
        selector.StartsWith("@font-face", StringComparison.InvariantCultureIgnoreCase) ||
        selector.StartsWith("@color-profile", StringComparison.InvariantCultureIgnoreCase)
        ;
}
