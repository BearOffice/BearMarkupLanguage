using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Text;
using BearMLLib.Configs;
using BearMLLib.Helpers;

namespace BearMLLib.Core
{
    internal static class LineAnalyzer
    {
        internal static bool IsGroupLine(string rawLine)
        {
            if (!rawLine.StartsWith(Identifier.Group)) return false;

            var name = rawLine.Remove(0, Identifier.Group.Length);
            if (name.IsNullOrWhiteSpace()) return false;
            // cannot treat '>>> name:' as group name line
            if (name.IndexOfWithEscape(Identifier.Key) != -1) return false;

            return true;
        }

        internal static bool IsKeyContentLine(string rawLine)
        {
            if (IsCommentLine(rawLine)) return false;

            var idKeyPos = rawLine.IndexOfWithEscape(Identifier.Key);
            if (idKeyPos == -1) return false;

            var key = rawLine[..idKeyPos];
            if (key.IsNullOrWhiteSpace() || key[0].IsWhiteSpace()) return false;

            // check if content is null or white space
            if (rawLine.Remove(0, idKeyPos + 1).IsNullOrWhiteSpace()) return false;

            return true;
        }

        internal static ContentType GetContentType(ReferList<string> raw)
        {
            var firstLine = raw[0];
            if (!IsKeyContentLine(firstLine)) throw new ArgumentException();

            var idKeyPos = firstLine.IndexOfWithEscape(Identifier.Key);
            var firstLineContent = firstLine[(idKeyPos + 1)..].TrimStartAndEnd();

            if (raw.Count == 1) return ContentType.EscapedValue;

            if (IsLiterialValueType(firstLineContent, raw[1..]))
                return ContentType.LiterialValue;
            else if (IsFoldedListType(firstLineContent, raw[1..]))
                return ContentType.FoldedList;
            else if (IsExpandedListType(firstLineContent, raw[1..]))
                return ContentType.ExpandedList;
            else
                return ContentType.EscapedValue;
        }

        internal static bool IsKeyAliasLine(string rawLine)
        {
            var line = rawLine.TrimEnd();
            if (!line.StartsWith(Identifier.KeyAliasL)
               || !line.EndsWith(Identifier.KeyAliasR))
                return false;

            if (line[1..^1].IsNullOrWhiteSpace()) return false;

            return true;
        }

        internal static bool IsCommentLine(string rawLine)
        {
            return rawLine.StartsWith(Identifier.Comment);
        }

        internal static bool IsLiterialValueType(string firstLine, ReferList<string> leftRaw)
        {
            if (leftRaw.Count == 0) return false;

            firstLine = firstLine.TrimStartAndEnd();
             
            if (firstLine == Identifier.MultiLineValue.ToString())
            {
                var depth = LiterialValueParser.GetDepthOfValue(leftRaw);
                if (depth != 0)
                    return true;
            }

            return false;
        }
        
        internal static bool IsExpandedListType(string firstLine, ReferList<string> leftRaw)
        {
            if (leftRaw.Count == 0) return false;

            firstLine = firstLine.TrimStartAndEnd();

            if (firstLine == Identifier.List.ToString())
            {
                var depth = ExpandedListParser.GetDepthOfList(leftRaw);
                if (depth != 0)
                    return true;
            }

            return false;
        }

        internal static bool IsFoldedListType(string firstLine, ReferList<string> leftRaw)
        {
            if (leftRaw.Count == 0) return false;

            firstLine = firstLine.TrimStartAndEnd();

            if (firstLine == Identifier.List.ToString())
            {
                var secondLine = leftRaw[0];
                if (!secondLine.HasDepthOf(1)) return false;

                secondLine = secondLine.TrimStartAndEnd();

                if (!secondLine.StartsWith(Identifier.FoldedListL) || 
                    !secondLine.EndsWith(Identifier.FoldedListR))
                    return false;

                return true;
            }

            return false;
        }

        internal static bool IsEscapedValueType(string firstLine, ReferList<string> leftRaw)
        {
            if (leftRaw.Count == 0)
            {
                if (firstLine.IsNullOrWhiteSpace())
                    return false;
                else
                    return true;
            }

            return !(IsExpandedListType(firstLine, leftRaw) || 
                     IsLiterialValueType(firstLine, leftRaw) || 
                     IsFoldedListType(firstLine, leftRaw));
        }

        internal static bool IsNullOrWhiteSpaceLine(string rawLine)
        {
            return rawLine.IsNullOrWhiteSpace();
        }
    }
}
