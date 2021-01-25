using System;
using System.Linq;
using System.Reflection;

namespace ConfigReadingLib
{
    internal interface IMember
    {
        public bool HasMember(string key);
        public Type GetMemberType(string key);
        public void SetValue(string key, object value);
        public object GetValue(string key);
    }

    internal class PropertyMember : IMember
    {
        private readonly object _object;
        private readonly PropertyInfo[] _properties;

        internal PropertyMember(object obj)
        {
            _object = obj;
            _properties = obj.GetType().GetProperties();
        }

        private PropertyInfo GetProperty(string key)
            => _properties.FirstOrDefault(x => string.Compare(x.Name, key, ignoreCase: true) == 0);

        public bool HasMember(string key)
            => GetProperty(key) != null;

        public Type GetMemberType(string key)
            => GetProperty(key).PropertyType;

        public void SetValue(string key, object value)
            => GetProperty(key).SetValue(_object, value);

        public object GetValue(string key)
            => GetProperty(key).GetValue(_object);
    }

    internal class FieldMember : IMember
    {
        private readonly object _object;
        private readonly FieldInfo[] _fields;

        internal FieldMember(object obj)
        {
            _object = obj;
            _fields = obj.GetType().GetFields();
        }

        private FieldInfo GetField(string key)
            => _fields.FirstOrDefault(x => string.Compare(x.Name, key, ignoreCase: true) == 0);

        public bool HasMember(string key)
            => GetField(key) != null;

        public Type GetMemberType(string key)
            => GetField(key).FieldType;

        public void SetValue(string key, object value)
            => GetField(key).SetValue(_object, value);

        public object GetValue(string key)
            => GetField(key).GetValue(_object);
    }
}
