using System;
using System.Collections.Generic;
using System.Text;

namespace BearMarkupLanguage.Text;

internal enum EscapeLevel
{
    Low,
    Middle,
    // high below
    High,
    CollapsedBasic,
    BlockKey,
    Key
}
