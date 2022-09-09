using System;
using System.Collections.Generic;
using System.Text;

namespace BearMarkupLanguage.Text;

// Identifier
internal static class ID
{
    internal const char Key = ':';
    internal const char Comment = '#';
    internal const char KeyAliasL = '[';
    internal const char KeyAliasR = ']';
    internal const char KeyAliasSplit = '|';
    internal const char LiteralElementSymbol = '@';
    internal const char EndOfLine = '|';
    internal const char ExpandedDicSymbol = '$';
    internal const char ExpandedListNode = '-';
    internal const char CollapsedListNodeL = '[';
    internal const char CollapsedListNodeR = ']';
    internal const char CollapsedDicNodeL = '{';
    internal const char CollapsedDicNodeR = '}';
    internal const char CollapsedBasicElementNode = '"';
    internal const char CollapsedElementSplitSymbol = ',';
    internal const char BlockL = '>';
    internal const char BlockR = '<';
    internal const string Indent = "  ";
    internal const string DefaultBlockKey = "default";
    internal const string EmptySymbol = "null";
}
