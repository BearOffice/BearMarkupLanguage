using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BearMLLib.Core.Helpers;
using BearMLLib.Configs;
using BearMLLib.Helpers;
using BearMLLib.Serialization;

namespace BearMLLib.Core
{
    internal class Reader
    {
        private readonly ReferList<string> _raw;
        private int _currentLinePos;

        internal Reader()
        {
            _raw = new ReferList<string>(new string[0]);
            _currentLinePos = 0;
        }

        internal Reader(string[] raw)
        {
            _raw = new ReferList<string>(raw);
            _currentLinePos = 0;
        }

        internal Reader(string path)
        {
            _raw = new ReferList<string>(File.ReadAllLines(path));
            _currentLinePos = 0;
        }

        internal ConfigsContainer GenerateConfigs()
        {
            ErrorHandler.This.Message += ThrowError;

            var container = new ConfigsContainer();
            var startPos = 0;

            if (_raw.Count == 0 || LineAnalyzer.IsGroupLine(_raw[startPos]))
            {
                // create empty default group at start.
                var defaultGroup = GroupParser.ParseFromRaw(new ReferList<string>(new string[0]), true, out _);
                container.GroupsDic.Add(defaultGroup.Name, defaultGroup);
                container.OrderedLine.Add(new TaggedLine(true, defaultGroup.Name));
            }
            else
            {
                var defaultGroup = GroupParser.ParseFromRaw(_raw[startPos..], true, out var groupDepth);
                container.GroupsDic.Add(defaultGroup.Name, defaultGroup);
                container.OrderedLine.Add(new TaggedLine(true, defaultGroup.Name));

                startPos += groupDepth + 1;
            }

            for (var i = startPos; i < _raw.Count; i++)
            {
                _currentLinePos = i;

                if (LineAnalyzer.IsGroupLine(_raw[i]))
                {
                    var group = GroupParser.ParseFromRaw(_raw[i..], false, out var groupDepth);
                    container.GroupsDic.Add(group.Name, group);
                    container.OrderedLine.Add(new TaggedLine(true, group.Name));

                    i += groupDepth;
                }
                else
                {
                    CheckLine(_raw[i]);
                    container.OrderedLine.Add(new TaggedLine(false, _raw[i]));
                }
            }

            ErrorHandler.This.Message -= ThrowError;

            return container;
        }

        private static void CheckLine(string rawLine)
        {
            if (!(LineAnalyzer.IsNullOrWhiteSpaceLine(rawLine) || LineAnalyzer.IsCommentLine(rawLine)))
                ErrorHandler.This.Add(ErrorType.InvalidLine, 0, rawLine);
        }

        private void ThrowError(ErrorArgs args)
        {
            switch (args.Type)
            {
                case ErrorType.InvalidLine:
                    throw new InvalidLineException($"Line {_currentLinePos + args.LineOffset + 1} is invalid. " +
                        $"{args.Message} cannot be recognized.");
                case ErrorType.InvalidConfig:
                    throw new InvalidConfigException("Config specified is invalid. " + args.Message);
                case ErrorType.TypeNotSupport:
                    throw new TypeNotSupportException(args.Message);
            }
        }
    }
}
