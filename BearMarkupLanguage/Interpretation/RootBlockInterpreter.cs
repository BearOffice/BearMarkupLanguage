using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Elements.Helpers;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Interpretation.Helpers;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Interpretation;

internal static class RootBlockInterpreter
{
    internal static RootBlockResult Interprete(ReferList<string> lines)
    {
        var blocksDic = new OrderedDictionary<BlockKey, Block>();
        var keyValuesDic = new OrderedDictionary<Key, IBaseElement>();
        var taggedLines = new List<TaggedLine>();

        var isPrevKeyAlias = false;
        for (var i = 0; i < lines.Count; i++)
        {
            if (ContextInterpreter.IsKeyLine(lines[i]))
            {
                var keyResult = KeyInterpreter.Interprete(lines[..(i + 1)], 
                    out var hasAlias, out var commentLinesNum, out var idIndex);
                if (!keyResult.IsSuccess) return RootBlockResult.Fail(keyResult.Error);  // no need to change index
                var key = keyResult.Value;
                if (keyValuesDic.ContainsKey(key))
                    return RootBlockResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = i,
                        CharIndex = ID.Indent.Length,
                        Message = $"Key name '{key.Name}' is not unique."
                    });

                taggedLines.RemoveRange(taggedLines.Count - commentLinesNum, commentLinesNum);

                var valueLineAndBelow = lines[i..];
                if (valueLineAndBelow[0].Length == idIndex + 1)
                    valueLineAndBelow[0] = "";
                else
                    valueLineAndBelow[0] = lines[i][(idIndex + 1)..].TrimStartAndEnd();  // get value and trim it

                var result = ContextInterpreter.ContentInterprete(valueLineAndBelow, out var endAtIndex);
                if (!result.IsSuccess) return RootBlockResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = result.Error.LineIndex + i,
                    CharIndex = result.Error.CharIndex,
                    Message = result.Error.Message
                });

                // -------- generate tagged lines --------
                var keyMetasNum = hasAlias ? 1 : 0;
                keyMetasNum += commentLinesNum;
                var keyLines = lines[(i - keyMetasNum)..(i + 1)];
                keyLines[^1] = keyLines[^1][..idIndex];

                var valueLines = lines[i..(i + endAtIndex + 1)];
                if (valueLines[0].Length == idIndex + 1)
                    valueLines[0] = "";
                else
                    valueLines[0] = lines[i][(idIndex + 1)..];

                taggedLines.Add(new TaggedLine
                {
                    LineType = LineType.KeyValuePair,
                    Key = key,
                    KeyLines = keyLines,
                    ValueLines = valueLines
                });
                keyValuesDic.Add(key, result.Value);
                i += endAtIndex;
                isPrevKeyAlias = false;
                continue;
            }

            if (isPrevKeyAlias)
                return RootBlockResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = i - 1,
                    CharIndex = 0,
                    Message = "Invalid key alias location."
                });

            if (ContextInterpreter.IsBlankLine(lines[i]))
            {
                taggedLines.Add(new TaggedLine { LineType = LineType.Blank, Line = lines[i] });
                continue;
            }

            if (ContextInterpreter.IsCommentLine(lines[i]))
            {
                taggedLines.Add(new TaggedLine { LineType = LineType.Comment, Line = lines[i] });
                continue;
            }

            if (ContextInterpreter.IsKeyAliasLine(lines[i]))
            {
                isPrevKeyAlias = true;
                continue;
            }

            if (ContextInterpreter.IsBlockLine(lines[i]))
            {
                var blockKey = BlockKeyInterprete(lines[..(i + 1)], out var keyLinesNum);
                if (!blockKey.HasValue) return RootBlockResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = i,
                    CharIndex = 0,
                    Message = "Block name cannot be empty or white space."
                });
                if (blocksDic.ContainsKey(blockKey.Value))
                    return RootBlockResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = i,
                        CharIndex = 0,
                        Message = $"Block name '{blockKey.Value.Name}' is not unique."
                    });

                taggedLines.RemoveRange(taggedLines.Count - keyLinesNum + 1, keyLinesNum - 1);

                var keyLines = lines[(i - keyLinesNum + 1)..(i + 1)];

                if (i == lines.Count - 1)
                {
                    blocksDic.Add(blockKey.Value, new Block
                    {
                        BlocksDic = new OrderedDictionary<BlockKey, Block>(),
                        KeyValuesDic = new OrderedDictionary<Key, IBaseElement>(),
                        TaggedLines = new List<TaggedLine>()
                    });

                    taggedLines.Add(new TaggedLine
                    {
                        LineType = LineType.Block,
                        BlockKey = blockKey.Value,
                        KeyLines = keyLines,
                        ValueLines = null
                    });
                    continue;
                }
                else
                {
                    var endIndex = GetBlockEndIndex(lines[(i + 1)..]);
                    var valueLines = lines[(i + 1)..(i + endIndex + 2)];  // i + endIndex + 2 -> i + 1 + endIndex + 1
                    var result = BlockInterpreter.Interprete(valueLines);
                    if (!result.IsSuccess) return RootBlockResult.Fail(new InvalidFormatExceptionArgs
                    {
                        LineIndex = result.Error.LineIndex + 1 + i,  // 1 -> start below block key line
                        CharIndex = result.Error.CharIndex,
                        Message = result.Error.Message
                    });

                    taggedLines.Add(new TaggedLine
                    {
                        LineType = LineType.Block,
                        BlockKey = blockKey.Value,
                        KeyLines = keyLines,
                        ValueLines = valueLines
                    });
                    blocksDic.Add(blockKey.Value, result.Value);
                    i += endIndex + 1;  // 1 -> block key line
                    continue;
                }
            }

            if (ContextInterpreter.IsNestedBlockLine(lines[i]))
            {
                return RootBlockResult.Fail(new InvalidFormatExceptionArgs
                {
                    LineIndex = i,
                    CharIndex = 0,
                    Message = "Root block cannot contain nested block."
                });
            }

            return RootBlockResult.Fail(new InvalidFormatExceptionArgs
            {
                LineIndex = i,
                CharIndex = 0,
                Message = "Invalid line."
            });
        }

        if (isPrevKeyAlias)
            return RootBlockResult.Fail(new InvalidFormatExceptionArgs
            {
                LineIndex = lines.Count - 1,
                CharIndex = 0,
                Message = "Invalid key alias location."
            });

        return RootBlockResult.Success(new RootBlock
        {
            BlocksDic = blocksDic,
            KeyValuesDic = keyValuesDic,
            TaggedLines = taggedLines
        });
    }

    private static BlockKey? BlockKeyInterprete(ReferList<string> keyLinesAndAbove, out int keyLinesNum)
    {
        keyLinesNum = 1;
        var keyName = GetBlockKey(keyLinesAndAbove[^1]);
        if (keyName.IsNullOrWhiteSpace())
            return null;

        if (keyLinesAndAbove.Count == 1)
            return new BlockKey { Name = keyName };

        var comment = default(string);
        for (var i = keyLinesAndAbove.Count - 2; i >= 0; i--)  // -2 -> Start at last 2nd line
        {
            if (ContextInterpreter.IsCommentLine(keyLinesAndAbove[i]))
            {
                if (comment is null)
                    comment = GetComment(keyLinesAndAbove[i]);
                else
                    comment = comment + '\n' + GetComment(keyLinesAndAbove[i]);
                keyLinesNum++;
            }
            else
            {
                break;
            }
        }

        return new BlockKey { Name = keyName, Comment = comment };
    }

    private static string GetBlockKey(string line)
    {
        return line.TrimEnd()[1..^1].TrimStartAndEnd();  // remove id '>' and '<'
    }

    private static int GetBlockEndIndex(ReferList<string> linesBelowKey)
    {
        for (var i = 0; i < linesBelowKey.Count; i++)
        {
            if (ContextInterpreter.IsBlockLine(linesBelowKey[i]))
            {
                _ = BlockKeyInterprete(linesBelowKey[..(i + 1)], out var keyLinesNum);
                return i - keyLinesNum;
            }
        }

        return linesBelowKey.Count - 1;
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
