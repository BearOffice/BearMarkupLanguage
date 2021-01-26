using System;
using System.Reflection;

namespace ConfigReadingLib.Member
{
    internal class PropertyMember : IMember
    {
        private readonly object _object;
        private readonly PropertyInfo _propertyinfo;

        public Type MemberType
        {
            get => _propertyinfo.PropertyType;
        }

        internal PropertyMember(object obj, PropertyInfo propertyinfo)
        {
            _object = obj;
            _propertyinfo = propertyinfo;
        }

        public void SetValue(object value)
            => _propertyinfo.SetValue(_object, value);

        public object GetValue()
            => _propertyinfo.GetValue(_object);
    }
}
