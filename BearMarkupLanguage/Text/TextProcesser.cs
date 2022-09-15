using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Helpers;

namespace BearMarkupLanguage.Text;

internal static class TextProcesser
{
    private static readonly string Indent = ID.Indent;

    private static readonly (char, EscapeLevel)[] _escapeChars = new[]
    {
        ('\n', EscapeLevel.Low), ('\r', EscapeLevel.Low), ('\t', EscapeLevel.Low), ('\f', EscapeLevel.Low),
        ('\\', EscapeLevel.Middle),
        (ID.Key, EscapeLevel.BlockKey),
        (ID.Key, EscapeLevel.Key), (ID.Comment, EscapeLevel.Key)
    };

    internal static bool IsEscapechar(this char ch, EscapeLevel level)
    {
        foreach (var (e, l) in _escapeChars)
        {
            if (e == ch && l <= level)
            {
                if (level <= EscapeLevel.Middle)
                    return true;
                else if (l < EscapeLevel.Middle || l == level)
                    return true;
            }
        }

        return false;
    }

    internal static bool IsMultiLine(this string input)
    {
        foreach (var ch in input)
        {
            if (IsEscapechar(ch, EscapeLevel.Low)) return true;
        }

        return false;
    }

    internal static string[] WeldingArrayWith(this string[] array, string[] weldedArray, string part)
    {
        var tempArr = new string[array.Length + weldedArray.Length - 1];

        for (var i = 0; i < array.Length - 1; i++)
        {
            tempArr[i] = array[i];
        }

        tempArr[array.Length - 1] = array[^1] + part + weldedArray[0];

        for (var i = 1; i < weldedArray.Length; i++)
        {
            tempArr[i + array.Length - 1] = weldedArray[i];
        }

        return tempArr;
    }

    internal static string Repeat(this string input, int count)
    {
        var sb = new StringBuilder(input);

        for (var i = 0; i < count; i++)
        {
            sb.Append(input);
        }

        return sb.ToString();
    }

    internal static string Repeat(this char input, int count)
    {
        if (count < 0) return "";

        var sb = new StringBuilder();
        sb.Append(input);

        for (var i = 0; i < count; i++)
        {
            sb.Append(input);
        }

        return sb.ToString();
    }

    internal static string[] SplitByLF(this string input)
    {
        return input.Replace("\r\n", "\n")
                    .Split(new[] { '\n', '\r' });
    }

    internal static string ConcatWithLF(this string[] input)
    {
        if (input.Length == 0) return "";

        var sb = new StringBuilder();
        foreach (var str in input.SkipLast(1))
        {
            sb.AppendLine(str);
        }
        sb.Append(input[^1]);

        return sb.ToString();
    }

    internal static bool HasDepthOf(this string input, int depth)
    {
        return depth == 0 || input.StartsWith(Repeat(Indent, depth - 1));
    }

    internal static string IncrOrDecrDepth(this string input, int num)
    {
        if (num == 0)
        {
            return input;
        }
        else if (num > 0)
        {
            return Repeat(Indent, num - 1) + input;
        }
        else
        {
            if (HasDepthOf(input, -num))
                return input[(-num * Indent.Length)..];
            else if (input.IsNullOrWhiteSpace())
                return "";
            else
                throw new ArgumentOutOfRangeException("Input does not have enough depth to decrease.");
        }
    }

    internal static string[] IncrOrDecrDepth(this string[] input, int num)
    {
        if (num == 0) return (string[])input.Clone();

        var tempArr = new string[input.Length];

        for (var i = 0; i < input.Length; i++)
        {
            tempArr[i] = IncrOrDecrDepth(input[i], num);
        }

        return tempArr;
    }

    internal static string[] IncrOrDecrDepth(this ReferList<string> input, int num)
    {
        var tempArr = new string[input.Count];

        for (var i = 0; i < input.Count; i++)
        {
            tempArr[i] = IncrOrDecrDepth(input[i], num);
        }

        return tempArr;
    }

    internal static string TrimStartAndEnd(this string input)
    {
        return input.TrimStart().TrimEnd();
    }

    internal static bool StartsWith(this string input, char value, bool ignoreWhiteSpace = false)
    {
        if (ignoreWhiteSpace)
            return input.TrimStart().StartsWith(value);
        else
            return input.StartsWith(value);
    }

    internal static bool StartsWith(this string input, string value, bool ignoreWhiteSpace = false)
    {
        if (ignoreWhiteSpace)
            return input.TrimStart().StartsWith(value);
        else
            return input.StartsWith(value);
    }

    internal static bool EndsWith(this string input, char value, bool ignoreWhiteSpace = false)
    {
        if (ignoreWhiteSpace)
            return input.TrimEnd().EndsWith(value);
        else
            return input.EndsWith(value);
    }

    internal static bool EndsWith(this string input, string value, bool ignoreWhiteSpace = false)
    {
        if (ignoreWhiteSpace)
            return input.TrimEnd().EndsWith(value);
        else
            return input.EndsWith(value);
    }

    internal static bool IsNullOrEmpty(this string input)
    {
        return string.IsNullOrEmpty(input);
    }

    internal static bool IsNullOrWhiteSpace(this string input)
    {
        return string.IsNullOrWhiteSpace(input);
    }

    internal static bool IsWhiteSpace(this char input)
    {
        return char.IsWhiteSpace(input);
    }

    internal static int IndexOfWithEscape(this string input, char ch)
    {
        var chPos = -1;

        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\')
            {
                i++;
                continue;
            }

            if (input[i] == ch)
            {
                chPos = i;
                break;
            }
        }

        return chPos;
    }

    internal static bool ContainsCharWithEscape(this string input, char ch)
    {
        var index = IndexOfWithEscape(input, ch);

        return index != -1;
    }

    internal static void ForWithEscape(this string input, Action<int, char> action)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\')
            {
                i++;
                continue;
            }

            action(i, input[i]);
        }
    }

    internal static bool ContainsEscapeChar(this string input, EscapeLevel level)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (IsEscapechar(input[i], level))
            {
                return true;
            }
        }

        return false;
    }

    internal static string EscapeChar(this string input, char ch)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == ch)
            {
                var sb = new StringBuilder();
                sb.Append(input, 0, i);

                do
                {
                    sb.Append('\\');
                    sb.Append(ch);

                    i++;

                    var lastPos = i;
                    while (i < input.Length)
                    {
                        if (input[i] == ch) break;

                        i++;
                    }
                    sb.Append(input, lastPos, i - lastPos);

                } while (i < input.Length);

                return sb.ToString();
            }
        }

        return input;
    }

    internal static string Escape(this string input, EscapeLevel level)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (IsEscapechar(input[i], level))
            {
                var sb = new StringBuilder();
                sb.Append(input, 0, i);

                do
                {
                    sb.Append('\\');
                    var ch = input[i];
                    ch = ch switch
                    {
                        '\n' => 'n',
                        '\r' => 'r',
                        '\t' => 't',
                        '\f' => 'f',
                        _ => ch
                    };
                    sb.Append(ch);

                    i++;

                    var lastPos = i;
                    while (i < input.Length)
                    {
                        ch = input[i];
                        if (IsEscapechar(ch, level)) break;

                        i++;
                    }
                    sb.Append(input, lastPos, i - lastPos);

                } while (i < input.Length);

                return sb.ToString();
            }
        }

        return input;
    }

    internal static string Unescape(this string input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\')
            {
                var sb = new StringBuilder();
                sb.Append(input, 0, i);

                do
                {
                    i++;

                    if (i < input.Length)
                    {
                        var ch = input[i];
                        ch = ch switch
                        {
                            'n' => '\n',
                            'r' => '\r',
                            't' => '\t',
                            'f' => '\f',
                            _ => ch
                        };
                        sb.Append(ch);

                        i++;
                    }

                    var lastPos = i;
                    while (i < input.Length && input[i] != '\\')
                    {
                        i++;
                    }
                    sb.Append(input, lastPos, i - lastPos);

                } while (i < input.Length);

                return sb.ToString();
            }
        }

        return input;
    }
}
