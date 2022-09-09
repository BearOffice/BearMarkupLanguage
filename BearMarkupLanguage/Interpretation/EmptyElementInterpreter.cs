using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal class EmptyElementInterpreter : IInterpreter
{
    public ElementResult Interprete(string[] lines, ParseMode mode)
    {
        return ElementResult.Success(new EmptyElement());
    }
}
