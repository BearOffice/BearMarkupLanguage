using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal class ListElementInterpreter : IInterpreter
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

        if (CollapsedElementHelper.FindElementEndIndex(line, 0, ID.CollapsedListNodeR) != line.Length - 1)
            return ElementResult.Fail(new InvalidFormatExceptionArgs
            {
                LineIndex = 0,
                CharIndex = 0,
                Message = "The format of this element is not valid."
            }); ;

        var tempList = new List<IBaseElement>();

        var isSplitted = true;
        var isEmpty = true;
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

                tempList.Add(result.Value);
                i = index;
                isSplitted = false;
                isEmpty = false;
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

                    var result = new EmptyElementInterpreter().Interpret(null, ParseMode.Collapse);
                    if (!result.IsSuccess) return ElementResult.PassToParent(result, 0, i);

                    tempList.Add(result.Value);
                    i = i + ID.EmptySymbol.Length - 1;
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
                if (isSplitted)
                    return ElementResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = 0,
                        CharIndex = i,
                        Message = "Unnecessary split symbol."
                    });
                else
                    isSplitted = true;
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

        if (!isEmpty && isSplitted)
            return ElementResult.Fail(new InvalidFormatExceptionArgs
            {
                LineIndex = 0,
                Message = "Unnecessary split symbol."
            });

        return ElementResult.Success(new ListElement(tempList));
    }

    private static ElementResult InterpretExpanded(string[] lines)
    {
        var refLines = new ReferList<string>(lines);
        var tempList = new List<IBaseElement>();

        for (var i = 0; i < lines.Length; i++)
        {
            if (ContextInterpreter.IsBlankLine(lines[i])) continue;
            if (!IsExpandedListNode(lines[i]))
                return ElementResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = i,
                    CharIndex = 0,
                    Message = "Invalid line."
                });
            
            // remove node
            if (lines[i].Length == 1)
                refLines[i] = "";
            else
                refLines[i] = refLines[i][1..];

            var result = ContextInterpreter.InterpretContent(refLines[i..], out var endAtIndex);
            if (!result.IsSuccess) 
                return ElementResult.PassToParent(result, i, 0);

            tempList.Add(result.Value);
            i += endAtIndex;
        }

        return ElementResult.Success(new ListElement(tempList));
    }

    private static bool IsExpandedListNode(string line)
    {
        if (line.Length < 1) return false;
        return line[0] == ID.ExpandedListNode;
    }
}
