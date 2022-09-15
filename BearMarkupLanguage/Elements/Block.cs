using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Elements;

internal class Block : RootBlock 
{
    internal override string[] ParseToLiteral(bool clearParse = false)
    {
        if (!_clearParseMode && !clearParse) return ParseToLiteralRefTag(incr: 1);

        var tempList = new List<string>();
        tempList.AddRange(KeyValuesParse());

        if (KeyValuesDic.Count > 0 && BlocksDic.Count > 0)
            tempList.Add("");

        tempList.AddRange(BlockParse().IncrOrDecrDepth(1));

        return tempList.ToArray();
    }
}
