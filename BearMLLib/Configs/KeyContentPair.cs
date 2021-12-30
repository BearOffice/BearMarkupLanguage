using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMLLib.Helpers;
using BearMLLib.Core;
using BearMLLib.Text;

namespace BearMLLib.Configs
{
    internal class KeyContentPair
    {
        private Key _key;
        private IContent _content;
        private ReferList<string> _raw;
        private bool _isChanged;
        internal Key Key { get => _key; set { _isChanged = true; _key = value; } }
        internal IContent Content { get => _content; set { _isChanged = true; _content = value; } }
        internal ReferList<string> Raw => BuildRaw();

        internal KeyContentPair(Key key, IContent content)
        {
            _key = key;
            _content = content;
            _raw = default;
            _isChanged = true;
        }

        internal KeyContentPair(Key key, IContent content, ReferList<string> raw)
        {
            _key = key;
            _content = content;
            _raw = raw;
            _isChanged = false;
        }

        private ReferList<string> BuildRaw()
        {
            var contentType = ContentTypeSelector.GetContentType(Content);

            // rebuild raw if content type do not fit the format rules
            // expand list need a special treat.
            if (_isChanged || contentType != Content.Type || contentType == ContentType.ExpandedList)
            {
                var keyRaw = KeyParser.ParseToRaw(Key);

                var contentRaw = IContentParser.GetParser(contentType)
                                               .ParseToRaw(ContentTypeSelector.GetContent(Content));

                _raw = new ReferList<string>(ConcatArrayJoint(keyRaw, contentRaw, Identifier.Key + " "));
                _isChanged = false;

                return _raw;
            }
            else
            {
                return _raw;
            }
        }

        /* 
         * ListA [abc, def]
         * ListB [ghi, jkl, mno]
         * ConcatListJoint(ListA, ListB, " : ") -> [abc, def : ghi, jkl, mno]
         */
        private static string[] ConcatArrayJoint(string[] arrayL, string[] arrayR, string joint)
        {
            if (arrayL.Length <= 0 || arrayR.Length <= 0)
                throw new ArgumentOutOfRangeException();

            return arrayL[0..^1].Concat(new[] { arrayL[^1] + joint + arrayR[0] }).Concat(arrayR[1..]).ToArray();
        }
    }
}
