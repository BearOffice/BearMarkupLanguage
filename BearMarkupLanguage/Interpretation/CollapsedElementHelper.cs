using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal static class CollapsedElementHelper
{
    internal static int FindElementEndIndex(string line, int startIndex)
    {
        return FindElementEndIndex(line, startIndex, FindPairNodeSymbol(line[startIndex]));
    }

    internal static int FindElementEndIndex(string line, int startIndex, char endSymbol)
    {
        var startSymbol = line[startIndex];

        var depth = 0;
        var inCollapsedBasicElem = false;
        for (var i = startIndex; i < line.Length; i++)
        {
            if (inCollapsedBasicElem && line[i] == '\\')
            {
                i++;
                continue;
            }

            if (line[i] == ID.CollapsedBasicElementNode)
                inCollapsedBasicElem = !inCollapsedBasicElem;

            if (!inCollapsedBasicElem)
            {
                if (line[i] == startSymbol) depth++;

                if (line[i] == endSymbol)
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
        }

        return -1;
    }

    internal static bool IsValidStartNode(char ch)
    {
        if (ch == ID.CollapsedListNodeL
            || ch == ID.CollapsedDicNodeL
            || ch == ID.CollapsedBasicElementNode)
            return true;
        else
            return false;
    }

    private static char FindPairNodeSymbol(char ch)
    {
        return ch switch
        {
            ID.CollapsedListNodeL => ID.CollapsedListNodeR,
            ID.CollapsedDicNodeL => ID.CollapsedDicNodeR,
            ID.CollapsedBasicElementNode => ID.CollapsedBasicElementNode,
            _ => throw new NotImplementedException(),
        };
    }

    internal static IInterpreter GetInterpreterWithNodeSymbol(char ch)
    {
        return ch switch
        {
            ID.CollapsedListNodeL=> new ListElementInterpreter(),
            ID.CollapsedDicNodeL => new DictionaryElementInterpreter(),
            ID.CollapsedBasicElementNode => new BasicElementInterpreter(),
            _ => throw new NotImplementedException(),
        };
    }
}
