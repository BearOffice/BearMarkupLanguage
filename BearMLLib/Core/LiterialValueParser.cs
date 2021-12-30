using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMLLib.Configs;
using BearMLLib.Helpers;
using BearMLLib.Serialization.Structure;
using BearMLLib.Text;

namespace BearMLLib.Core
{
    internal class LiterialValueParser : IContentParser
    {
        public IContent ParseFromRaw(ReferList<string> raw, out int depth)
        {
            depth = GetDepthOfValue(raw[1..]);

            var value = raw[1..(depth + 1)].Select(item => item.Remove(0, Identifier.Indent.Length))
                                           .Aggregate("", (acc, i) => acc + i + '\n')[..^1];

            return new LiterialValue(new BasicValue(value));
        }

        public string[] ParseToRaw(IContent content)
        {
            var literialValue = content as LiterialValue;

            var raw = new List<string> { Identifier.MultiLineValue.ToString() };
            raw.AddRange(literialValue.BaseValue.PlainValue
                            .Replace("\r\n", "\n")
                            .Split(new[] { '\n', '\r' })
                            .Select(item => Identifier.Indent + item));

            return raw.ToArray();
        }

        // start from second line (skip the identifier)
        internal static int GetDepthOfValue(ReferList<string> raw)
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
