using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Interpretation;
using BearMarkupLanguage.Helpers;

namespace BearMarkupLanguage.Core;

internal class Reader
{
    private readonly string _path;

    internal Reader(string path)
    {
        _path = path;

        // Create blank file if needed.
        if (!File.Exists(path)) File.Create(path);
    }

    internal RootBlockResult Read(out string[] lines)
    {
        var tempList = new List<string>();

        using var reader = new StreamReader(_path);
        while (!reader.EndOfStream)
        {
            tempList.Add(reader.ReadLine());
        }

        lines = tempList.ToArray();
        return RootBlockInterpreter.Interprete(new ReferList<string>(lines));
    }
}
