using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BearMarkupLanguage.Elements.Helpers;

internal enum LineType
{
    Blank,
    Comment,
    KeyValuePair,
    Block,
    BlankBlock
}
