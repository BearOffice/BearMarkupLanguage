using System;
using System.Linq;
using System.Reflection;

namespace ConfigReadingLib.Member
{
    internal class PropertyMembers : IMembers
    {
        private readonly object _object;
        private readonly PropertyInfo[] _properties;

        internal PropertyMembers(object obj)
        {
            _object = obj;
            _properties = obj.GetType().GetProperties();
        }

        private PropertyInfo GetProperty(string key)
            => _properties.FirstOrDefault(x => string.Compare(x.Name, key, ignoreCase: true) == 0);

        public IMember GetMember(string key)
            => new PropertyMember(_object, GetProperty(key));

        public bool HasMember(string key)
            => GetProperty(key) != null;
    }
}
