using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation.Helpers;

internal class ElementResult : Result<IBaseElement, InvalidFormatExceptionArgs>
{
    internal ElementResult(IBaseElement success) : base(success) { }
    internal ElementResult(InvalidFormatExceptionArgs fail) : base(fail) { }

    internal static ElementResult Success(IBaseElement success)
    {
        return new ElementResult(success);
    }

    internal static ElementResult Fail(InvalidFormatExceptionArgs args)
    {
        return new ElementResult(args);
    }

    internal static ElementResult PassToParent(ElementResult result, int indexIncr, int charIncr)
    {
        if (result.IsSuccess)
            return result;
        else
            return new ElementResult(result.Error with
            {
                LineIndex = indexIncr + result.Error.LineIndex,
                CharIndex = charIncr + result.Error.CharIndex,
            });
    }
}