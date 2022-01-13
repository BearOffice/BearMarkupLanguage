using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Text
{
    internal static class TextProcesser
    {
        private static readonly string Indent = Identifier.Indent;

        private static readonly (char, EscapeLevel)[] _escapeChars = new[]
        {
            ('\n', EscapeLevel.L), ('\r', EscapeLevel.L), ('\t', EscapeLevel.L), ('\f', EscapeLevel.L),
            ('\\', EscapeLevel.M),
            (Identifier.Key, EscapeLevel.HG),
            (Identifier.Key, EscapeLevel.HK), (Identifier.Comment, EscapeLevel.HK),
            (Identifier.FoldedListNode, EscapeLevel.HL)
        };

        internal static bool IsEscapechar(this char ch, EscapeLevel level)
        {
            foreach (var (e, l) in _escapeChars)
            {
                if (e == ch && l <= level)
                {
                    if (level <= EscapeLevel.M)
                        return true;
                    else if (l < EscapeLevel.M || l == level)
                        return true;
                }
            }

            return false;
        }

        internal static bool IsMultiLine(this string input)
        {
            foreach (var ch in input)
            {
                if (IsEscapechar(ch, EscapeLevel.L)) return true;
            }

            return false;
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

        internal static bool HasDepthOf(this string input, int depth)
        {
            return depth == 0 || input.StartsWith(Repeat(Indent, depth - 1));
        }
        
        internal static string IncreaseOrDecreaseDepth(this string input, int num)
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
                if (HasDepthOf(input, num))
                    return input[(num * Indent.Length)..];
                else
                    throw new ArgumentOutOfRangeException("Input does not have enough depth to decrease.");
            }
        }

        internal static string TrimStartAndEnd(this string input)
        {
            return input.TrimStart().TrimEnd();
        }

        internal static bool StartsWith(this string input, char value, bool ignoreWhiteSpace)
        {
            if (ignoreWhiteSpace)
                return input.TrimStart().StartsWith(value);
            else
                return input.StartsWith(value);
        }

        internal static bool StartsWith(this string input, string value, bool ignoreWhiteSpace)
        {
            if (ignoreWhiteSpace)
                return input.TrimStart().StartsWith(value);
            else
                return input.StartsWith(value);
        }

        internal static bool EndsWith(this string input, char value, bool ignoreWhiteSpace)
        {
            if (ignoreWhiteSpace)
                return input.TrimEnd().EndsWith(value);
            else
                return input.EndsWith(value);
        }

        internal static bool EndsWith(this string input, string value, bool ignoreWhiteSpace)
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
}
