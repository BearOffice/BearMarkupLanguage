using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Helpers;
using BearMLLib.Configs;
using BearMLLib.Core.Helpers;

namespace BearMLLib.Core
{
    internal class ConfigsContainer
    {
        internal OrderedDictionary<string, Group> GroupsDic { get; }
        internal List<TaggedLine> OrderedLine { get; }
        internal string[] Raw => BuildRaw();

        internal ConfigsContainer()
        {
            GroupsDic = new OrderedDictionary<string, Group>();
            OrderedLine = new List<TaggedLine>();
        }

        private string[] BuildRaw()
        {
            var raw = new List<string>();

            foreach (var location in OrderedLine)
            {
                if (location.IsItemName)
                    raw.AddRange(GroupParser.ParseToRaw(GroupsDic[location.Text]));
                else
                    raw.Add(location.Text);
            }

            return raw.ToArray();
        }
    }
}
