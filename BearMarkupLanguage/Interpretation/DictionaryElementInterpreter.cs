using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal class DictionaryElementInterpreter : IInterpreter
{
    public ElementResult Interpret(string[] lines, ParseMode mode)
    {
        return mode switch
        {
            ParseMode.Collapse => InterpretCollapsed(lines[0]),
            ParseMode.Expand => InterpretExpanded(lines),
            _ => throw new NotImplementedException(),
        };
    }

    private static ElementResult InterpretCollapsed(string line)
    {
        line = line.TrimEnd();

        if (CollapsedElementHelper.FindElementEndIndex(line, 0, ID.CollapsedDicNodeR) != line.Length - 1)
            return ElementResult.Fail(new InvalidFormatExceptionArgs
            {
                LineIndex = 0,
                CharIndex = 0,
                Message = "The format of this element is not valid."
            }); ;

        var tempDic = new OrderedDictionary<BasicElement, IBaseElement>();

        var isSplitted = true;
        var isEmpty = true;
        var tempKey = default(BasicElement);
        var valueEntry = false;
        for (var i = 1; i < line.Length - 1; i++)
        {
            var ch = line[i];
            if (ch.IsWhiteSpace()) continue;

            if (CollapsedElementHelper.IsValidStartNode(ch))
            {
                if (!isSplitted) return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = 0,
                    CharIndex = i,
                    Message = "Missing split symbol."
                });
                if (tempKey is null && (ch == ID.CollapsedListNodeL || ch == ID.CollapsedDicNodeL))
                    return ElementResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = 0,
                        CharIndex = i,
                        Message = "Invalid key type."
                    });
                if (tempKey is not null && !valueEntry)
                    return ElementResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = 0,
                        CharIndex = i,
                        Message = "Missing split symbol of key value pair."
                    });

                var index = CollapsedElementHelper.FindElementEndIndex(line, i);
                if (index == -1) return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = 0,
                    CharIndex = i,
                    Message = "Bracket not closed."
                });

                string content;
                if (ch == ID.CollapsedBasicElementNode)
                    content = line[(i + 1)..index];  // remove "" for basic element
                else
                    content = line[i..(index + 1)];

                var result = CollapsedElementHelper.GetInterpreterWithNodeSymbol(ch)
                                                   .Interpret(new[] { content }, ParseMode.Collapse);
                if (!result.IsSuccess) return ElementResult.PassToParent(result, 0, i);

                if (tempKey is null)
                {
                    tempKey = (BasicElement)result.Value;

                    // check if key already existed.
                    if (tempDic.ContainsKey(tempKey)) return ElementResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = 0,
                        CharIndex = i,
                        Message = "Key must be unique."
                    });
                }
                else
                {
                    tempDic.Add(tempKey, result.Value);
                    tempKey = null;
                    valueEntry = false;
                    isSplitted = false;
                    isEmpty = false;
                }

                i = index;
            }
            else if (ch == ID.EmptySymbol[0])
            {
                if (line[i..].StartsWith(ID.EmptySymbol))
                {
                    if (!isSplitted) return ElementResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = 0,
                        CharIndex = i,
                        Message = "Missing split symbol."
                    });
                    if (tempKey is null) return ElementResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = 0,
                        CharIndex = i,
                        Message = "Invalid key type. Key cannot be null."
                    });
                    if (tempKey is not null && !valueEntry)
                        return ElementResult.Fail(new InvalidFormatExceptionArgs
                        {
                            LineIndex = 0,
                            CharIndex = i,
                            Message = "Missing key symbol."
                        });

                    var result = new EmptyElementInterpreter().Interpret(null, ParseMode.Collapse);
                    if (!result.IsSuccess) return ElementResult.PassToParent(result, 0, i);

                    tempDic.Add(tempKey, result.Value);
                    i = i + ID.EmptySymbol.Length - 1;
                    tempKey = null;
                    valueEntry = false;
                    isSplitted = false;
                    isEmpty = false;
                }
                else
                {
                    return ElementResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = 0,
                        CharIndex = i,
                        Message = "Unknown character."
                    });
                }
            }
            else if (ch == ID.CollapsedElementSplitSymbol)
            {
                if (valueEntry) return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = 0,
                    CharIndex = i,
                    Message = "Missing value."
                });

                if (isSplitted) return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = 0,
                    CharIndex = i,
                    Message = "Unnecessary split symbol."
                });
                else
                    isSplitted = true;
            }
            else if (ch == ID.Key)
            {
                if (valueEntry) return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = 0,
                    CharIndex = i,
                    Message = "Unnecessary key symbol."
                });

                if (tempKey is null && !valueEntry)
                    return ElementResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = 0,
                        CharIndex = i,
                        Message = "Missing key."
                    });
                else
                    valueEntry = true;
            }
            else
            {
                return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = 0,
                    CharIndex = i,
                    Message = "Unknown character."
                });
            }
        }

        if (valueEntry) return ElementResult.Fail(new InvalidFormatExceptionArgs
        {
            LineIndex = 0,
            Message = "Missing value."
        });
        if (!isEmpty && isSplitted) return ElementResult.Fail(new InvalidFormatExceptionArgs
        {
            LineIndex = 0,
            Message = "Unnecessary split symbol."
        });

        return ElementResult.Success(new DictionaryElement(tempDic));
    }

    private static ElementResult InterpretExpanded(string[] lines)
    {
        var refLines = new ReferList<string>(lines);
        var tempDic = new OrderedDictionary<BasicElement, IBaseElement>();

        for (var i = 0; i < lines.Length; i++)
        {
            if (ContextInterpreter.IsBlankLine(lines[i])) continue;
            if (!IsDictionaryNode(lines[i]))
                return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = i,
                    CharIndex = 0,
                    Message = "Invalid line."
                });

            var key = GetKey(lines[i], out var idIndex);
            if (key is null) return ElementResult.Fail(new InvalidFormatExceptionArgs
            {
                LineIndex = i,
                CharIndex = 0,
                Message = "Key cannot be empty or white space in expanded dictionary."
            });
            // check if key already existed.
            if (tempDic.ContainsKey(new BasicElement(key)))
                return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = i,
                    CharIndex = 0,
                    Message = "Key must be unique."
                });

            // key.Length + id's length
            if (idIndex + 1 == lines[i].Length)
                refLines[i] = "";
            else
                refLines[i] = refLines[i][(idIndex + 1)..];

            var result = ContextInterpreter.InterpretContent(refLines[i..], out var endAtIndex);
            if (!result.IsSuccess)
                return ElementResult.PassToParent(result, i, 0);

            tempDic.Add(new BasicElement(key), result.Value);
            i += endAtIndex;
        }

        return ElementResult.Success(new DictionaryElement(tempDic));
    }

    private static bool IsDictionaryNode(string line)
    {
        return ContextInterpreter.IsKeyLine(line);
    }

    private static string GetKey(string line, out int idIndex)
    {
        idIndex = line.IndexOfWithEscape(ID.Key);
        var key = line[0..idIndex].TrimEnd().Unescape();

        if (key.IsNullOrWhiteSpace())
            return null;
        else
            return key;
    }
}
