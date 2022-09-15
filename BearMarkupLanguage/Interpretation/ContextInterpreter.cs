using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal static class ContextInterpreter
{
    internal static bool IsBlankLine(string line)
    {
        return line.IsNullOrWhiteSpace();
    }

    internal static bool IsCommentLine(string line)
    {
        return line.TrimStart().StartsWith(ID.Comment);
    }

    internal static bool IsKeyLine(string line)
    {
        if (IsBlankLine(line)) return false;
        if (line[0].IsWhiteSpace()) return false;

        if (IsCommentLine(line)) return false;
        if (!line.ContainsCharWithEscape(ID.Key)) return false;

        return true;
    }

    internal static bool IsKeyAliasLine(string line)
    {
        if (!line.StartsWith(ID.KeyAliasL)) return false;

        line = line.TrimEnd();
        if (!line.EndsWith(ID.KeyAliasR)) return false;

        return true;
    }

    internal static bool IsBlockLine(string line)
    {
        if (IsKeyLine(line)) return false;

        if (!line.StartsWith(ID.BlockL)) return false;

        line = line.TrimEnd();
        if (!line.EndsWith(ID.BlockR)) return false;

        return true;
    }

    internal static bool IsNestedBlockLine(string line)
    {
        if (!line.HasDepthOf(1)) return false;
        if (line.Length == ID.Indent.Length) return false;
        return IsBlockLine(line[ID.Indent.Length..]);
    }

    // lines[0] without key
    internal static ElementResult ContentInterprete(ReferList<string> lines, out int endAtIndex)
    {
        if (lines[0].IsNullOrWhiteSpace())
        {
            for (var i = 1; i < lines.Count; i++)
            {
                if (lines[i].IsNullOrWhiteSpace()) continue;

                if (lines[i].HasDepthOf(1) && lines[i].Length > ID.Indent.Length)
                {
                    ElementResult result;
                    if (lines[i][2] == ID.ExpandedListNode)
                    {
                        endAtIndex = GetMultiLinesElementEndIndex(lines[i..]) + i;
                        result = new ListElementInterpreter()
                            .Interprete(lines[i..(endAtIndex + 1)].IncrOrDecrDepth(-1), ParseMode.Expand);
                    }
                    else if (lines[i][2] == ID.CollapsedListNodeL)
                    {
                        endAtIndex = 1;
                        result = new ListElementInterpreter().Interprete(new[] { lines[i][2..] }, ParseMode.Collapse);
                    }
                    else if (lines[i][2] == ID.CollapsedDicNodeL)
                    {
                        endAtIndex = 1;
                        result = new DictionaryElementInterpreter().Interprete(new[] { lines[i][2..] }, ParseMode.Collapse);
                    }
                    else
                    {
                        break;
                    }

                    return ElementResult.PassToParent(result, i, ID.Indent.Length);
                }
                else
                {
                    break;
                }
            }

            endAtIndex = 0;
            return new EmptyElementInterpreter().Interprete(null, ParseMode.Expand);
        }
        else
        {
            if (lines[0].TrimStartAndEnd() == ID.LiteralElementSymbol.ToString())
            {
                if (IsLiteralElement(lines[1..], out var endIndex))
                {
                    endAtIndex = 1 + endIndex;

                    if (endAtIndex != 1)
                    {
                        var result = new BasicElementInterpreter().Interprete(lines[1..endAtIndex].IncrOrDecrDepth(-1), ParseMode.Expand);
                        return ElementResult.PassToParent(result, 1, ID.Indent.Length);
                    }
                    else
                    {
                        var result = new EmptyElementInterpreter().Interprete(null, ParseMode.Expand);
                        return ElementResult.PassToParent(result, 1, ID.Indent.Length);
                    }
                }
            }
            else if (lines[0].TrimStartAndEnd() == ID.ExpandedDicSymbol.ToString())
            {
                for (var i = 1; i < lines.Count; i++)
                {
                    if (lines[i].IsNullOrWhiteSpace()) continue;

                    if (lines[i].HasDepthOf(1) && lines[i].Length > ID.Indent.Length
                        && IsKeyLine(lines[i][2..]))
                    {
                        endAtIndex = GetMultiLinesElementEndIndex(lines[i..]) + i;
                        var result = new DictionaryElementInterpreter()
                            .Interprete(lines[i..(endAtIndex + 1)].IncrOrDecrDepth(-1), ParseMode.Expand);
                        return ElementResult.PassToParent(result, i, ID.Indent.Length);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            endAtIndex = 0;
            return new BasicElementInterpreter().Interprete(new[] { lines[0].TrimStartAndEnd() }, ParseMode.Collapse);
        }
    }

    private static bool IsLiteralElement(ReferList<string> linesBelowKey, out int endAtIndex)
    {
        endAtIndex = -1;

        for (var i = 0; i < linesBelowKey.Count; i++)
        {
            if (!linesBelowKey[i].HasDepthOf(1))
            {
                if (linesBelowKey[i].TrimEnd() == ID.EndOfLine.ToString())
                {
                    endAtIndex = i;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        return false;
    }

    private static int GetMultiLinesElementEndIndex(ReferList<string> linesBelowKey)
    {
        for (var i = 0; i < linesBelowKey.Count; i++)
        {
            if ((!linesBelowKey[i].HasDepthOf(1) && !IsBlankLine(linesBelowKey[i]))
                || IsCommentLine(linesBelowKey[i]) || IsNestedBlockLine(linesBelowKey[i]))
                return i - 1;
        }

        return linesBelowKey.Count - 1;
    }
}
