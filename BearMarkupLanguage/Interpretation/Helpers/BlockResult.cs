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

internal class BlockResult : Result<Block, InvalidFormatExceptionArgs>
{
    internal BlockResult(Block success) : base(success) { }
    internal BlockResult(InvalidFormatExceptionArgs fail) : base(fail) { }

    internal static BlockResult Success(Block success)
    {
        return new BlockResult(success);
    }

    internal static BlockResult Fail(InvalidFormatExceptionArgs args)
    {
        return new BlockResult(args);
    }
}