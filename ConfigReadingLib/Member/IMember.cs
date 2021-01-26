using System;

namespace ConfigReadingLib.Member
{
    internal interface IMember
    {
        public Type MemberType { get; }
        public void SetValue(object value);
        public object GetValue();
    }
}