using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using BearMLLib.Configs;
using BearMLLib.Helpers;
using BearMLLib.Serialization.Structure;
using BearMLLib.Text;
using BearMLLib.Core.Helpers;

namespace BearMLLib.Core
{
    internal class FoldedListParser : IContentParser
    {
        public IContent ParseFromRaw(ReferList<string> raw, out int depth)
        {
            depth = 1;
            var node = new Node(BuildNodes(raw[1]));

            return new FoldedList(node);
        }

        public string[] ParseToRaw(IContent content)
        {
            var foldedList = content as FoldedList;

            return BuildRaw(foldedList.Node);
        }

        private static List<INode> BuildNodes(string rawline)
        {
            var nodes = default(List<INode>);

            if (IsNestedListRaw(rawline))
            {
                var lists = new List<string>();
                var listLine = rawline.TrimStartAndEnd()[1..^1];

                var startPos = 0;
                var inDepth = 0;
                var inNode = false;
                var splitCount = 0;
                listLine.ForWithEscape((i, ch) =>
                {
                    if (ch == Identifier.FoldedListNode)
                    {
                        if (inNode)
                            inNode = false;
                        else
                            inNode = true;
                    }

                    if (!inNode)
                    {
                        if (ch == Identifier.FoldedListL)
                        {
                            if (inDepth == 0)
                            {
                                startPos = i;
                            }

                            inDepth++;
                        }
                        else if (ch == Identifier.FoldedListR)
                        {
                            inDepth--;

                            if (inDepth == 0)
                            {
                                lists.Add(listLine[startPos..(i + 1)]);
                                startPos = 0;
                                splitCount--;
                            }
                        }
                    }

                    if (inDepth == 0)
                    {
                        if (ch == Identifier.FoldedListSplit)
                            splitCount++;
                        if (splitCount > 0 || !(ch.IsWhiteSpace() || ch == Identifier.FoldedListSplit ||
                            ch == Identifier.FoldedListL || ch == Identifier.FoldedListR ||
                            ch == Identifier.FoldedListNode))
                            ErrorHandler.This.Add(ErrorType.InvalidLine, 1, rawline);
                    }

                    if (inDepth < 0) ErrorHandler.This.Add(ErrorType.InvalidLine, 1, rawline);
                });

                if (inDepth != 0) ErrorHandler.This.Add(ErrorType.InvalidLine, 1, rawline);

                nodes = lists.Select(item => new Node(BuildNodes(item)) as INode).ToList();
            }
            else if (IsSingleListRaw(rawline))
            {
                nodes = GetElements(rawline);
            }
            else
            {
                ErrorHandler.This.Add(ErrorType.InvalidLine, 1, rawline);
            }

            return nodes;
        }

        private static List<INode> GetElements(string listLine)
        {
            if (!IsSingleListRaw(listLine))
                ErrorHandler.This.Add(ErrorType.InvalidLine, 1, listLine);

            var elements = new List<string>();
            var elementLine = listLine.TrimStartAndEnd()[1..^1];  // Remove [" and "]

            var startPos = 0;
            var inNode = false;
            var splitCount = 0;
            elementLine.ForWithEscape((i, ch) =>
            {
                if (ch == Identifier.FoldedListNode)
                {
                    if (!inNode)
                    {
                        inNode = true;
                        startPos = i;
                    }
                    else
                    {
                        inNode = false;
                        elements.Add(elementLine[startPos..(i + 1)]);
                        startPos = 0;
                        splitCount--;
                    }
                }

                if (!inNode)
                {
                    if (ch == Identifier.FoldedListSplit) splitCount++;
                    if (splitCount > 0 ||
                        !(ch.IsWhiteSpace() || ch == Identifier.FoldedListSplit ||
                          ch == Identifier.FoldedListNode))
                        ErrorHandler.This.Add(ErrorType.InvalidLine, 1, listLine);
                }
            });

            if (inNode) ErrorHandler.This.Add(ErrorType.InvalidLine, 1, listLine);

            return elements.Select(item => new Node(new BasicValue(item[1..^1])) as INode).ToList();
        }

        private static bool IsSingleListRaw(string rawLine)
        {
            // [ and ] need to be escaped in regex
            if (Regex.IsMatch(rawLine,
                $@"^\s*\{Identifier.FoldedListL}.*\{Identifier.FoldedListR}\s*$"))
                return true;

            return false;
        }

        private static bool IsNestedListRaw(string rawLine)
        {
            // [ and ] need to be escaped in regex
            if (Regex.IsMatch(rawLine,
                $@"^\s*\{Identifier.FoldedListL}\s*\{Identifier.FoldedListL}.*" +
                $@"\{Identifier.FoldedListR}\s*{Identifier.FoldedListSplit}?\s*\{Identifier.FoldedListR}\s*$"))
                return true;

            return false;
        }

        private static string[] BuildRaw(INode node)
        {
            return new[] { Identifier.List.ToString(), Identifier.Indent + BuildFoldedListLine(node) };

            static string BuildFoldedListLine(INode node)
            {
                var sb = new StringBuilder();

                if (node.Nodes != null)
                {
                    sb.Append(Identifier.FoldedListL);

                    if (node.Nodes.Count > 0)
                    {
                        foreach (var n in node.Nodes)
                        {
                            var nodeValue = BuildFoldedListLine(n);
                            sb.Append(nodeValue)
                              .Append(Identifier.FoldedListSplit)
                              .Append(' ');
                        }
                        sb.Remove(sb.Length - 2, 2);
                    }

                    sb.Append(Identifier.FoldedListR);
                }
                else
                {
                    var contentValue = node.BasicValue.PlainValue;
                    sb.Append(Identifier.FoldedListNode)
                      .Append(contentValue.Escape(EscapeLevel.HL))
                      .Append(Identifier.FoldedListNode);
                }

                return sb.ToString();
            }
        }
    }
}
