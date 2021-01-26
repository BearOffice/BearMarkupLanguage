using System;
using System.Linq;
using System.Reflection;

namespace ConfigReadingLib.Member
{
    internal class FieldMembers : IMembers
    {
        private readonly object _object;
        private readonly FieldInfo[] _fields;

        internal FieldMembers(object obj)
        {
            _object = obj;
            _fields = obj.GetType().GetFields();
        }

        private FieldInfo GetField(string key)
            => _fields.FirstOrDefault(x => string.Compare(x.Name, key, ignoreCase: true) == 0);

        public IMember GetMember(string key)
            => new FieldMember(_object, GetField(key));

        public bool HasMember(string key)
            => GetField(key) != null;
    }
}
