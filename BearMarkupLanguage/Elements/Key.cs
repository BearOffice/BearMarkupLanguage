using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Elements;

internal readonly struct Key : IEquatable<Key>
{
    internal string Name { get; init; }
    internal string[] Aliases { get; init; }
    internal string Comment { get; init; }

    internal static bool IsValidName(string name)
    {
        return !name.IsNullOrWhiteSpace();
    }

    internal static bool IsValidAlias(string alias)
    {
        return Regex.IsMatch(alias, @"^@?[a-zA-Z_][a-zA-Z_0-9]*$");
    }

    internal string[] ParseToLiteral()
    {
        var tempList = new List<string>();

        if (Comment is not null)
        {
            tempList.AddRange(Comment.SplitByLF()
                                     .Select(item => ID.Comment + item));
        }

        if (Aliases is not null && Aliases.Length > 0)
        {
            var sb = new StringBuilder();
            sb.Append(ID.KeyAliasL);

            foreach (var alias in Aliases.SkipLast(1))
            {
                sb.Append(alias).Append(ID.KeyAliasSplit);
            }

            sb.Append(Aliases[^1]);

            sb.Append(ID.KeyAliasR);
            tempList.Add(sb.ToString());
        }

        tempList.Add(Name.Escape(EscapeLevel.Key));

        return tempList.ToArray();
    }

    public bool Equals(Key other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (obj is Key key)
            return Equals(key);
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static bool operator ==(Key keyL, Key keyR)
    {
        return keyL.Equals(keyR);
    }

    public static bool operator !=(Key keyL, Key keyR)
    {
        return !keyL.Equals(keyR);
    }
}
