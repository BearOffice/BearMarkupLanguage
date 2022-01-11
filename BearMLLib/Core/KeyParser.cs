using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMLLib.Configs;
using BearMLLib.Text;
using BearMLLib.Helpers;

namespace BearMLLib.Core
{
    internal static class KeyParser
    {
        internal static Key ParseFromRaw(ReferList<string> raw, out int depth)
        {
            if (!LineAnalyzer.IsKeyContentLine(raw[^1])) throw new ArgumentException();

            depth = 0;

            var key = GetKeyFromRaw(raw[^1]);
            var keyAlias = default(string);
            var comment = default(string);

            if (raw.Count > 1 && LineAnalyzer.IsKeyAliasLine(raw[^2]))
            {
                keyAlias = GetKeyAliasFromRaw(raw[^2]);
                depth++;
            }

            if (keyAlias == null)
            {
                if (raw.Count > 1 && LineAnalyzer.IsCommentLine(raw[^2]))
                {
                    comment = GetCommentFromRaw(raw[..^1], out var cDepth);
                    depth += cDepth;
                }
            }
            else
            {
                if (raw.Count > 2 && LineAnalyzer.IsCommentLine(raw[^3]))
                {
                    comment = GetCommentFromRaw(raw[..^2], out var cDepth);
                    depth += cDepth;
                }
            }

            return new Key(key, keyAlias, comment);
        }

        internal static string[] ParseToRaw(Key key)
        {
            var raw = new List<string>();

            if (key.Comment != null)
                raw.AddRange(key.Comment.Replace("\r\n", "\n")
                                        .Split(new[] { '\n', '\r' })
                                        .Select(item => Identifier.Comment + " " + item));

            if (key.AliasName != null)
                raw.Add(Identifier.KeyAliasL + " " + key.AliasName + " " + Identifier.KeyAliasR);

            raw.Add(key.Name.Escape(EscapeLevel.HK));

            return raw.ToArray();
        }

        private static string GetKeyFromRaw(string rawLine)
        {
            var idKeyPos = rawLine.IndexOfWithEscape(Identifier.Key);
            return rawLine[0..idKeyPos].TrimEnd().Unescape();
        }

        private static string GetKeyAliasFromRaw(string rawLine)
        {
            var aliasName = rawLine.TrimEnd()[1..^1].TrimStartAndEnd();  // remove '[' and ']'

            return aliasName;
        }

        private static string GetCommentFromRaw(ReferList<string> raw, out int depth)
        {
            var comments = new List<string>();

            for (var i = raw.Count - 1; i >= 0; i--)
            {
                var currentLine = raw[i];
                if (!LineAnalyzer.IsCommentLine(currentLine))
                    break;

                comments.Add(currentLine);  // Get comment lines backward
            }

            depth = comments.Count;
            comments.Reverse();

            var sb = new StringBuilder();

            foreach (var line in comments)
            {
                var comment = line[1..];  // line[1..] -> remove '#'
                sb.Append(comment).Append('\n');
            }
            sb.Remove(sb.Length - 1, 1);
            
            return sb.ToString();
        }
    }
}
