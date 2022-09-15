using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Interpretation.Helpers;

namespace BearMarkupLanguage.Interpretation;

internal interface IInterpreter
{
    public ElementResult Interprete(string[] lines, ParseMode mode);
}
