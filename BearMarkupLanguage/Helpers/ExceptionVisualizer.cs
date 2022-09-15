using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Helpers;

internal static class ExceptionVisualizer
{
    internal static string BuildMessage(string errLine, int lineIndex, int charIndex, string errMessage)
    {
        var sb = new StringBuilder();
        sb.Append("Line ").Append(lineIndex + 1).Append("  ");
        sb.Append("Character num ").Append(charIndex + 1).Append(':').Append('\n');

        var index = 0;
        if (charIndex <= 15)
        {
            if (errLine.Length > 0)
                sb.Append(errLine[..(charIndex + 1)]);
            index = charIndex;
        }
        else
        {
            sb.Append("...");
            sb.Append(errLine[(charIndex - 12)..(charIndex + 1)]);
            index += 15;  // "...".length + 12
        }

        if (errLine.Length > charIndex + 1)
        {
            if (errLine.Length - (charIndex + 1) < 20)
            {
                sb.Append(errLine[(charIndex + 1)..]);
            }
            else
            {
                sb.Append(errLine[(charIndex + 1)..(charIndex + 18)]);
                sb.Append("...");
            }
        }

        sb.Append('\n');
        sb.Append(' '.Repeat(index - 1)).Append("^ ");
        sb.Append(errMessage);

        return sb.ToString();
    }
}
