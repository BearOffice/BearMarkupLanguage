using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConfReaderLib
{
    internal interface IMemberInfo
    {
        public Type MemberType { get; }
        public bool Find(string key);
        public void SetValue(object obj, object value);
        public object GetValue(object obj);
    }

    internal class PropInfo : IMemberInfo
    {
        public Type MemberType { get { return _property.PropertyType; } }
        private readonly PropertyInfo[] _properties;
        private PropertyInfo _property;

        internal PropInfo(object obj)
        {
            _properties = obj.GetType().GetProperties();
        }
        public bool Find(string key)
        {
            _property = _properties.FirstOrDefault(x => string.Compare(x.Name, key, ignoreCase: true) == 0);
            if (_property == null) return false;
            return true;
        }
        public void SetValue(object obj, object value)
            => _property.SetValue(obj, value);

        public object GetValue(object obj)
            => _property.GetValue(obj);
    }

    internal class FldInfo : IMemberInfo
    {
        public Type MemberType { get { return _field.FieldType; } }
        private readonly FieldInfo[] _fields;
        private FieldInfo _field;

        internal FldInfo(object obj)
        {
            _fields = obj.GetType().GetFields();
        }
        public bool Find(string key)
        {
            _field = _fields.FirstOrDefault(x => string.Compare(x.Name, key, ignoreCase: true) == 0);
            if (_field == null) return false;
            return true;
        }

        public void SetValue(object obj, object value)
            => _field.SetValue(obj, value);

        public object GetValue(object obj)
            => _field.GetValue(obj);
    }
}
