using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BearMLLib.Text;

namespace BearMLLib.Configs
{
    internal struct Key : IEquatable<Key>
    {
        internal string Name { get; }
        internal string AliasName { get; }
        internal string Comment { get; }

        internal Key(string name, string aliasName, string comment)
        {
            if (!IsValidName(name))
                throw new InvalidConfigException("The format of Key is invalid.");

            if (aliasName != null && !IsValidAliasName(aliasName))
                throw new InvalidConfigException("The format of Key alias is invalid.");

            Name = name;
            AliasName = aliasName;
            Comment = comment;
        }

        internal static bool IsValidName(string name)
        {
            return !name.IsNullOrWhiteSpace() && !name[0].IsWhiteSpace();
        }

        internal static bool IsValidAliasName(string aliasName)
        {
            return Regex.IsMatch(aliasName, @"^@?[a-zA-Z_][a-zA-Z_0-9]*$");
        }

        public bool Equals(Key other)
        {
            if (Name == other.Name)
                return true;
            else
                return false;
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
}
