using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BearMarkupLanguage.Elements;

namespace BearMarkupLanguage.Core;

internal class Writer
{
    private readonly string _path;

    internal Writer(string path)
    {
        _path = path;
    }

    
    internal void Write(RootBlock rootBlock, bool clearParse = false)
    {
        var literals = rootBlock.ParseToLiteral(clearParse);

        using var writer = new StreamWriter(_path, false);
        foreach(var literal in literals)
        {
            writer.WriteLine(literal);
        }
    }
}
