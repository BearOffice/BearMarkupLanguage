using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearMarkupLanguage.Conversion;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Elements;

internal class EmptyElement : IBaseElement
{
    public ParseMode PreferredParseMode => ParseMode.Expand;

    public object ConvertTo(Type targetType, IConversionProvider[] providers)
    {
        return default;
    }

    public string[] ParseToLiteral(ParseMode mode)
    {
        return mode switch
        {
            ParseMode.Expand => new[] { "" },
            ParseMode.Collapse => new[] { ID.EmptySymbol },
            _ => throw new NotImplementedException(),
        };
    }
}
