using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Configs;
using BearMLLib.Helpers;

namespace BearMLLib.Core
{
    internal interface IContentParser
    {
        IContent ParseFromRaw(ReferList<string> raw, out int depth);
        string[] ParseToRaw(IContent content);

        internal static IContentParser GetParser(ContentType contentType)
        {
            return contentType switch
            {
                ContentType.EscapedValue => new EscapedValueParser(),
                ContentType.LiterialValue => new LiterialValueParser(),
                ContentType.ExpandedList => new ExpandedListParser(),
                ContentType.FoldedList => new FoldedListParser(),
                _ => throw new NotSupportedException()
            };
        }
    }
}
