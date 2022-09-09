using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearMarkupLanguage.Helpers;


namespace BearMarkupLanguage.Elements.Helpers;

internal class TaggedLine
{
    internal LineType LineType { get; init; }
    internal string Line { get; init; }  // for empty and comment line only

    internal BlockKey BlockKey { get; init; }
    internal Key Key { get; init; }
    internal ReferList<string> KeyLines { get; set; }
    internal ReferList<string> ValueLines { get; set; }
}
