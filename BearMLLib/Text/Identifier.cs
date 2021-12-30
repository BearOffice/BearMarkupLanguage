using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Text
{
    internal static class Identifier
    {
        internal const char Key = ':';
        internal const char Comment = '#';
        internal const char KeyAliasL = '[';
        internal const char KeyAliasR = ']';
        internal const char MultiLineValue = '|';
        internal const char List = '>';
        internal const char ExpandedListNode = '-';
        internal const char FoldedListL = '[';
        internal const char FoldedListR = ']';
        internal const char FoldedListNode = '"';
        internal const char FoldedListSplit = ',';
        internal const string Group = ">>>";
        internal const string Indent = "  ";
        internal const string DefaultGroupName = "default";
    }
}
