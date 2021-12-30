using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMLLib.Configs;
using BearMLLib.Helpers;
using BearMLLib.Core.Helpers;
using BearMLLib.Serialization.Structure;
using BearMLLib.Text;

namespace BearMLLib.Core
{
    internal class ExpandedListParser : IContentParser
    {
        public IContent ParseFromRaw(ReferList<string> raw, out int depth)
        {
            depth = GetDepthOfList(raw[1..]);
            var node = new Node(BuildNodes(raw[1..(depth + 1)]));

            return new ExpandedList(node);
        }

        public string[] ParseToRaw(IContent content)
        {
            var expandedList = content as ExpandedList;

            return BuildRaw(expandedList.Node);
        }

        private static List<INode> BuildNodes(ReferList<string> raw)
        {
            var nodes = new List<INode>();

            for (var i = 0; i < raw.Count; i++)
            {
                if (LineAnalyzer.IsNullOrWhiteSpaceLine(raw[i]))
                    continue;

                if (!IsNodeLine(raw[i]))
                    ErrorHandler.This.Add(ErrorType.InvalidLine, i + 1, raw[i]);

                var elementRaw = GetElementRaw(raw[i..], out var _);
                var contentType = GetContentType(elementRaw);
                var parser = IContentParser.GetParser(contentType);

                var content = parser.ParseFromRaw(RemoveIdentifier(elementRaw), out var depth);

                if (content is EscapedValue escapedValue)
                    nodes.Add(new Node(escapedValue.BaseValue));
                else if (content is LiterialValue literialValue)
                    nodes.Add(new Node(literialValue.BaseValue));
                else if (content is ExpandedList expandedList)
                    nodes.Add(expandedList.Node);
                else if (content is FoldedList foldedList)
                    nodes.Add(foldedList.Node);

                i += depth;
            }

            return nodes;
        }

        private static ReferList<string> GetElementRaw(ReferList<string> raw, out int depth)
        {
            depth = 0;

            foreach (var line in raw.Skip(1))
            {
                if (IsNodeLine(line)) break;
                depth++;
            }

            return new ReferList<string>(
                raw[..(depth + 1)].Select(item => item.Remove(0, Identifier.Indent.Length)).ToArray());
        }

        private static ReferList<string> RemoveIdentifier(ReferList<string> elementRaw)
        {
            var firstLine = new[] { elementRaw[0][1..] };

            if (elementRaw.Count == 1)
                return new ReferList<string>(firstLine);
            else
                return new ReferList<string>(firstLine.Concat(elementRaw[1..]).ToArray());
        }

        private static ContentType GetContentType(ReferList<string> elementRaw)
        {
            var firstLine = elementRaw[0];
            if (firstLine[0] != Identifier.ExpandedListNode) throw new ArgumentException();

            var firstLineContent = firstLine[1..].TrimStartAndEnd();

            if (elementRaw.Count == 1) return ContentType.EscapedValue;

            if (LineAnalyzer.IsLiterialValueType(firstLineContent, elementRaw[1..]))
                return ContentType.LiterialValue;
            else if (LineAnalyzer.IsFoldedListType(firstLineContent, elementRaw[1..]))
                return ContentType.FoldedList;
            else if (LineAnalyzer.IsExpandedListType(firstLineContent, elementRaw[1..]))
                return ContentType.ExpandedList;
            else
                return ContentType.EscapedValue;
        }

        private static bool IsNodeLine(string rawLine)
        {
            if (!rawLine.HasDepthOf(1)) return false;

            rawLine = rawLine.Remove(0, Identifier.Indent.Length);
            if (rawLine.Length < 1 || rawLine[0] != Identifier.ExpandedListNode) return false;

            var content = rawLine[1..];
            if (content.Length < 1 || content.IsNullOrWhiteSpace()) return false;

            return true;
        }

        private static string[] BuildRaw(INode node)
        {
            var raw = new List<string>();

            if (node.Nodes != null)
            {
                raw.Add(Identifier.List.ToString());

                if (node.Nodes.Count > 0)
                {
                    foreach (var n in node.Nodes)
                    {
                        // select expanded list or folded list that fits the format rule
                        var content = ContentTypeSelector.GetContent(n);
                        var nRaw = IContentParser.GetParser(content.Type).ParseToRaw(content);
                        raw.AddRange(AddIdentifier(nRaw).Select(item => Identifier.Indent + item));
                    }
                }
                else
                {
                    raw.Add(Identifier.Indent);  // allow empty node
                }
            }
            else
            {
                // select escaped value or literial value that fits the format rule
                var content = ContentTypeSelector.GetContent(node.BasicValue);
                var contentRaw = IContentParser.GetParser(content.Type).ParseToRaw(content);
                raw.AddRange(contentRaw);
            }

            return raw.ToArray();
        }

        private static string[] AddIdentifier(string[] contentRaw)
        {
            return ConcateFirstLine(Identifier.ExpandedListNode + " ", contentRaw);

            static string[] ConcateFirstLine(string firstLine, string[] array)
            {
                firstLine += array[0];

                if (array.Length == 1)
                    return new[] { firstLine };
                else
                    return new[] { firstLine }.Concat(array[1..]).ToArray();
            }
        }

        // start from second line (skip the identifier)
        internal static int GetDepthOfList(ReferList<string> raw)
        {
            var depth = 0;

            foreach (var line in raw)
            {
                if (line.HasDepthOf(1))
                    depth++;
                else
                    break;
            }

            return depth;
        }
    }
}
