using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BearMarkupLanguage.Interpretation.Helpers;

internal record InvalidFormatExceptionArgs
{
    public int LineIndex { get; init; }
    public int CharIndex { get; init; }
    public string Message { get; init; }
}
