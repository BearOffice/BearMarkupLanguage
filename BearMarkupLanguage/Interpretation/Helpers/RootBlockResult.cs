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

internal class RootBlockResult : Result<RootBlock, InvalidFormatExceptionArgs>
{
    internal RootBlockResult(RootBlock success) : base(success) { }
    internal RootBlockResult(InvalidFormatExceptionArgs fail) : base(fail) { }

    internal static RootBlockResult Success(RootBlock success)
    {
        return new RootBlockResult(success);
    }

    internal static RootBlockResult Fail(InvalidFormatExceptionArgs args)
    {
        return new RootBlockResult(args);
    }
}