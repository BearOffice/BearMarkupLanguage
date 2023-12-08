using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal class BasicElementInterpreter : IInterpreter
{
    public ElementResult Interpret(string[] lines, ParseMode mode)
    {
        return mode switch
        {
            ParseMode.Collapse => InterpretCollapsed(lines[0]),
            ParseMode.Expand => InterpretExpanded(lines),
            _ => throw new NotImplementedException(),
        };
    }

    private static ElementResult InterpretCollapsed(string line)
    {
        var literal = line.Unescape();
        return ElementResult.Success(new BasicElement(literal));
    }

    private static ElementResult InterpretExpanded(string[] lines)
    {
        return ElementResult.Success(new BasicElement(lines.ConcatWithLF()));
    }
}
