using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Text;
using BearMarkupLanguage.Elements.Helpers;
using System.Collections;

namespace BearMarkupLanguage.Elements;

internal class RootBlock
{
    protected bool _clearParseMode = false;
    internal OrderedDictionary<BlockKey, Block> BlocksDic { get; init; }
    internal OrderedDictionary<Key, IBaseElement> KeyValuesDic { get; init; }
    internal List<TaggedLine> TaggedLines { get; init; }

    internal virtual string[] ParseToLiteral(bool clearParse = false)
    {
        if (!_clearParseMode && !clearParse) return ParseToLiteralRefTag();

        var tempList = new List<string>();
        tempList.AddRange(KeyValuesParse());

        if (KeyValuesDic.Count > 0 && BlocksDic.Count > 0)
            tempList.Add("");

        tempList.AddRange(BlockParse());

        return tempList.ToArray();
    }

    protected string[] ParseToLiteralRefTag(int incr = 0)
    {
        var tempList = new List<string>();

        foreach (var tag in TaggedLines)
        {
            if (tag.LineType == LineType.Blank || tag.LineType == LineType.Comment)
            {
                tempList.Add(tag.Line);
            }
            else if (tag.LineType == LineType.Block)
            {
                var key = BlocksDic.Keys.First(k => k == tag.BlockKey);  // tag key may be diff from dic key (key alias changes etc)
                tempList.AddRange(BlockKeyParse(key, tag, incr));
                tempList.AddRange(BlockValueParse(BlocksDic[tag.BlockKey], tag, incr));
            }
            else if (tag.LineType == LineType.KeyValuePair)
            {
                var key = KeyValuesDic.Keys.First(k => k == tag.Key);
                var keyParsed = KeyParse(key, tag);
                var valueParsed = ValueParse(KeyValuesDic[tag.Key], tag);

                if (tag.ValueLines is not null)
                    tempList.AddRange(keyParsed.WeldingArrayWith(valueParsed, ID.Key.ToString()));
                else
                    tempList.AddRange(keyParsed.WeldingArrayWith(valueParsed, ID.Key + " "));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return tempList.ToArray();
    }

    protected string[] BlockParse()
    {
        var tempList = new List<string>();

        foreach ((var key, var block) in BlocksDic)
        {
            tempList.AddRange(BlockKeyParse(key));
            tempList.AddRange(BlockValueParse(block));
            tempList.Add("");
        }

        if (BlocksDic.Count > 0)
            tempList.RemoveAt(tempList.Count - 1);

        return tempList.ToArray();
    }

    private static string[] BlockKeyParse(BlockKey key, TaggedLine taggedLine = null, int incr = 0)
    {
        if (taggedLine is null || taggedLine.KeyLines is null)
        {
            return key.ParseToLiteral().IncrOrDecrDepth(incr);
        }
        else
        {
            return taggedLine.KeyLines.ToArray();
        }
    }

    private static string[] BlockValueParse(Block block, TaggedLine taggedLine = null, int incr = 0)
    {
        if (taggedLine is null || taggedLine.ValueLines is null)
        {
            var clearParse = taggedLine is null;
            return block.ParseToLiteral(clearParse).IncrOrDecrDepth(incr);
        }
        else
        {
            return taggedLine.ValueLines.ToArray();
        }
    }

    protected string[] KeyValuesParse()
    {
        var tempList = new List<string>();

        foreach ((var key, var elem) in KeyValuesDic)
        {
            var keyParsed = KeyParse(key);
            var valueParsed = ValueParse(elem);
            tempList.AddRange(keyParsed.WeldingArrayWith(valueParsed, ID.Key + " "));
            tempList.Add("");
        }

        if (KeyValuesDic.Count > 0)
            tempList.RemoveAt(tempList.Count - 1);

        return tempList.ToArray();
    }

    internal static string[] KeyParse(Key key, TaggedLine taggedLine = null)
    {
        if (taggedLine is null || taggedLine.KeyLines is null)
        {
            return key.ParseToLiteral();
        }
        else
        {
            return taggedLine.KeyLines.ToArray();
        }
    }

    internal static string[] ValueParse(IBaseElement element, TaggedLine taggedLine = null)
    {
        if (taggedLine is not null && taggedLine.ValueLines is not null)
            return taggedLine.ValueLines.ToArray();

        var tempList = new List<string>();

        var elemParseMode = element.PreferredParseMode;
        var elemLiteral = element.ParseToLiteral(elemParseMode);

        if (element is BasicElement)
        {
            switch (elemParseMode)
            {
                case ParseMode.Collapse:
                    tempList.Add(elemLiteral[0]);
                    break;
                case ParseMode.Expand:
                    tempList.Add(ID.LiteralElementSymbol.ToString());
                    tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
                    tempList.Add(ID.EndOfLine.ToString());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        else if (element is EmptyElement)
        {
            tempList.Add(elemLiteral[0]);
        }
        else if (element is DictionaryElement)
        {
            switch (elemParseMode)
            {
                case ParseMode.Collapse:
                    tempList.Add("");
                    tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
                    break;
                case ParseMode.Expand:
                    tempList.Add(ID.ExpandedDicSymbol.ToString());
                    tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        else
        {
            tempList.Add("");
            tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
        }

        return tempList.ToArray();
    }

    internal void SetClearParseStatus()
    {
        _clearParseMode = true;
    }
}
