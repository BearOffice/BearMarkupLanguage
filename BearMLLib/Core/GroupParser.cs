using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Configs;
using BearMLLib.Helpers;
using BearMLLib.Core.Helpers;
using BearMLLib.Text;

namespace BearMLLib.Core
{
    internal static class GroupParser
    {
        internal static Group ParseFromRaw(ReferList<string> raw, bool isDefault, out int depth)
        {
            if (!isDefault && !LineAnalyzer.IsGroupLine(raw[0])) throw new ArgumentException();

            var group = isDefault ? new Group() : new Group(GetGroupNameFromRaw(raw[0]), raw[0]);

            var rawOffset = isDefault ? 0 : 1;
            depth = rawOffset - 1;
            var checkFrom = rawOffset;
            for (var i = rawOffset; i < raw.Count; i++)
            {
                if (LineAnalyzer.IsGroupLine(raw[i]))
                {
                    foreach (var line in raw[checkFrom..i])
                    {
                        CheckLine(line, checkFrom);
                        group.OrderedLine.Add(new TaggedLine(false, line));
                    }

                    return group;
                }

                depth++;

                if (LineAnalyzer.IsKeyContentLine(raw[i]))
                {
                    var key = KeyParser.ParseFromRaw(raw[..(i + 1)], out var keyDepth);
                    var contentType = LineAnalyzer.GetContentType(raw[i..]);
                    var content = IContentParser.GetParser(contentType).ParseFromRaw(raw[i..], out var contentDepth);

                   
                    foreach (var line in raw[checkFrom..(i - keyDepth)])
                    {
                        CheckLine(line, checkFrom);
                        group.OrderedLine.Add(new TaggedLine(false, line));
                    }

                    var pair = new KeyContentPair(key, content, raw[(i - keyDepth)..(i + contentDepth + 1)]);
                    group.KeyContentPairsDic.Add(key.Name, pair);
                    group.OrderedLine.Add(new TaggedLine(true, key.Name));

                    i += contentDepth;
                    depth += contentDepth;
                    checkFrom = i + 1;
                }
            }

            if (checkFrom < raw.Count)
            {
                foreach(var line in raw[checkFrom..])
                {
                    CheckLine(line, checkFrom);
                    group.OrderedLine.Add(new TaggedLine(false, line));
                }
            }

            return group;
        }

        internal static string[] ParseToRaw(Group group)
        {
            var raw = new List<string>();

            if (group.FirstRawLine != null) raw.Add(group.FirstRawLine);

            foreach (var location in group.OrderedLine)
            {
                if (location.IsItemName)
                {
                    if (group.KeyContentPairsDic.ContainsKey(location.Text))
                        raw.AddRange(group.KeyContentPairsDic[location.Text].Raw);
                }
                else
                {
                    raw.Add(location.Text);
                }

            }

            return raw.ToArray();
        }

        private static string GetGroupNameFromRaw(string rawLine)
        {
            return rawLine.Remove(0, Identifier.Group.Length).TrimStartAndEnd().Unescape();
        }

        private static void CheckLine(string rawLine, int offset)
        {
            if (!(LineAnalyzer.IsNullOrWhiteSpaceLine(rawLine) || LineAnalyzer.IsCommentLine(rawLine)))
                ErrorHandler.This.Add(ErrorType.InvalidLine, offset + 1, rawLine);
        }
    }
}
