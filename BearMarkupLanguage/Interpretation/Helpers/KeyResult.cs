using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation.Helpers;

internal class KeyResult : Result<Key, InvalidFormatExceptionArgs>
{
    internal KeyResult(Key success) : base(success) { }
    internal KeyResult(InvalidFormatExceptionArgs fail) : base(fail) { }

    internal static KeyResult Success(Key success)
    {
        return new KeyResult(success);
    }

    internal static KeyResult Fail(InvalidFormatExceptionArgs args)
    {
        return new KeyResult(args);
    }
}