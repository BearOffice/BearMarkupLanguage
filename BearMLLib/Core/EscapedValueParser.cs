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
    internal class EscapedValueParser : IContentParser
    {
        public IContent ParseFromRaw(ReferList<string> raw, out int depth)
        {
            depth = 0;
            var value = RemoveIdentifier(raw)[0].TrimStartAndEnd().Unescape();
            return new EscapedValue(new BasicValue(value));
        }

        public string[] ParseToRaw(IContent content)
        {
            var escapedValue = content as EscapedValue;
            return new[] { escapedValue.BaseValue.PlainValue.Escape(EscapeLevel.HC) };
        }

        private static ReferList<string> RemoveIdentifier(ReferList<string> raw)
        {
            var idKeyPos = raw[0].IndexOfWithEscape(Identifier.Key);

            var firstLine = new[] { raw[0].Remove(0, idKeyPos + 1) };

            if (raw.Count == 1)
                return new ReferList<string>(firstLine);
            else
                return new ReferList<string>(firstLine.Concat(raw[1..]).ToArray());
        }
    }
}
