using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Helpers;
using BearMLLib.Core.Helpers;
using BearMLLib.Text;

namespace BearMLLib.Configs
{
    internal class Group
    {
        internal OrderedDictionary<string, KeyContentPair> KeyContentPairsDic { get; }
        internal List<TaggedLine> OrderedLine { get; }
        internal string Name { get; }
        internal bool IsDefault { get; }
        internal string FirstRawLine { get; }
        
        internal Group()
        {
            IsDefault = true;
            Name = Identifier.DefaultGroupName;
            FirstRawLine = null;
            KeyContentPairsDic = new OrderedDictionary<string, KeyContentPair>();
            OrderedLine = new List<TaggedLine>();
        }

        internal Group(string name)
        {
            IsDefault = false;
            Name = name;
            FirstRawLine = Identifier.Group + " " + name.Escape(EscapeLevel.HG);
            KeyContentPairsDic = new OrderedDictionary<string, KeyContentPair>();
            OrderedLine = new List<TaggedLine>();
        }

        internal Group(string name, string firstRawLine)
        {
            IsDefault = false;
            Name = name;
            FirstRawLine = firstRawLine;
            KeyContentPairsDic = new OrderedDictionary<string, KeyContentPair>();
            OrderedLine = new List<TaggedLine>();
        }
    }
}
