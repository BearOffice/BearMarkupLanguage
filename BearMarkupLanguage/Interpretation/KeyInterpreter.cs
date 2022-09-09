using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal static class KeyInterpreter
{
    internal static KeyResult Interprete(ReferList<string> keyLinesAndAbove, out bool hasAlias,
        out int commentLinesNum, out int idIndex)
    {
        hasAlias = false;
        commentLinesNum = 0;
        var keyName = GetKey(keyLinesAndAbove[^1], out idIndex);
        if (keyName.IsNullOrWhiteSpace())
            return KeyResult.Fail(new InvalidFormatExceptionArgs
            {
                LineIndex = keyLinesAndAbove.Count - 1,
                CharIndex = 0,
                Message = "Key cannot be empty or white space."
            });

        if (keyLinesAndAbove.Count == 1)
            return KeyResult.Success(new Key { Name = keyName });

        var comment = default(string);
        var keyAlias = default(string[]);

        // [^2]
        if (ContextInterpreter.IsKeyAliasLine(keyLinesAndAbove[^2]))
        {
            keyAlias = GetKeyAliases(keyLinesAndAbove[^2]);
            if (keyAlias is null)
                return KeyResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = keyLinesAndAbove.Count - 2,
                    CharIndex = 0,
                    Message = "Invalid key alias format."
                });

            hasAlias = true;
        }
        else if (ContextInterpreter.IsCommentLine(keyLinesAndAbove[^2]))
        {
            comment = GetComment(keyLinesAndAbove[^2]);
            commentLinesNum++;
        }
        else
        {
            return KeyResult.Success(new Key { Name = keyName });
        }


        if (keyLinesAndAbove.Count > 2)
        {
            for (var i = keyLinesAndAbove.Count - 3; i >= 0; i--)  // -3 -> Start at last 3rd line
            {
                if (!ContextInterpreter.IsCommentLine(keyLinesAndAbove[i])) break;

                if (comment is null)
                    comment = GetComment(keyLinesAndAbove[i]);
                else
                    comment = comment + '\n' + GetComment(keyLinesAndAbove[i]);
                commentLinesNum++;
            }
        }

        return KeyResult.Success(new Key { Name = keyName, Aliases = keyAlias, Comment = comment });
    }

    private static string GetKey(string line, out int idIndex)
    {
        idIndex = line.IndexOfWithEscape(ID.Key);
        return line[0..idIndex].TrimEnd().Unescape();
    }

    private static string[] GetKeyAliases(string line)
    {
        var aliases = line.TrimStartAndEnd()[1..^1].Split(ID.KeyAliasSplit);

        for (var i = 0; i < aliases.Length; i++)
        {
            aliases[i] = aliases[i].TrimStartAndEnd();
            if (!Key.IsValidAlias(aliases[i])) return null;
        }

        return aliases;
    }

    private static string GetComment(string line)
    {
        var comment = line.TrimStart();
        if (comment.Length == 1)  // as line.length == 1 is the case of "#"
            return "";
        else
            return comment[1..];
    }
}
