using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Elements;

internal struct BlockKey : IEquatable<BlockKey>
{
    internal string Name { get; init; }
    internal string Comment { get; init; }

    internal static bool IsValidName(string name)
    {
        return !name.IsNullOrWhiteSpace();
    }

    internal string[] ParseToLiteral()
    {
        var tempList = new List<string>();

        if (Comment is not null)
        {
            tempList.AddRange(Comment.SplitToLines()
                                     .Select(item => ID.Comment + item));
        }

        tempList.Add(ID.BlockL + " " + Name.Escape(EscapeLevel.Key) + " " + ID.BlockR);

        return tempList.ToArray();
    }

    public bool Equals(BlockKey other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (obj is BlockKey key)
            return Equals(key);
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static bool operator ==(BlockKey keyL, BlockKey keyR)
    {
        return keyL.Equals(keyR);
    }

    public static bool operator !=(BlockKey keyL, BlockKey keyR)
    {
        return !keyL.Equals(keyR);
    }
}
