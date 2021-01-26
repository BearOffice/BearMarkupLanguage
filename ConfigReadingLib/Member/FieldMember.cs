using System;
using System.Reflection;

namespace ConfigReadingLib.Member
{
    internal class FieldMember : IMember
    {
        private readonly object _object;
        private readonly FieldInfo _fieldInfo;

        public Type MemberType
        {
            get => _fieldInfo.FieldType;
        }

        internal FieldMember(object obj, FieldInfo fieldinfo)
        {
            _object = obj;
            _fieldInfo = fieldinfo;
        }

        public void SetValue(object value)
            => _fieldInfo.SetValue(_object, value);

        public object GetValue()
            => _fieldInfo.GetValue(_object);
    }
}
