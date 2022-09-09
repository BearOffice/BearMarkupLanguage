using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal class BasicElementInterpreter : IInterpreter
{
    public ElementResult Interprete(string[] lines, ParseMode mode)
    {
        return mode switch
        {
            ParseMode.Collapse => CollapsedInterprete(lines[0]),
            ParseMode.Expand => ExpandedInterprete(lines),
            _ => throw new NotImplementedException(),
        };
    }

    private static ElementResult CollapsedInterprete(string line)
    {
        var literal = line.Unescape();
        return ElementResult.Success(new BasicElement(literal));
    }

    private static ElementResult ExpandedInterprete(string[] lines)
    {
        var sb = lines.SkipLast(1).Aggregate(new StringBuilder(), (acc, item) => acc.AppendLine(item));
        if (lines.Length > 0) sb.Append(lines[^1]);
        return ElementResult.Success(new BasicElement(sb.ToString()));
    }
}
