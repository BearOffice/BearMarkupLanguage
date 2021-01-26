using System;

namespace ConfigReadingLib.Member
{
    internal interface IMembers
    {
        public bool HasMember(string key);
        public IMember GetMember(string key);
    }
}
