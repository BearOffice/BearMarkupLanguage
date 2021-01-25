using System;

namespace ConfigReadingLib
{
    /// <summary>
    /// Class ConfigInfo contains the infomation of key, value and comment.
    /// </summary>
    public struct ConfigInfo
    {
        private string _key;
        private string _value;
        private string _comment;

        /// <summary>
        /// Key can not be set as null or white space.
        /// </summary>
        /// <exception cref="BadConfException"></exception>
        public string Key
        {
            get => _key ?? throw new BadConfException($"The key is null.");
            set => _key = !string.IsNullOrWhiteSpace(value) ?
               value : throw new BadConfException($"The key can not be set as null or white space.");
        }

        /// <summary>
        /// Value can not be set as null or white space.
        /// </summary>
        /// <exception cref="BadConfException"></exception>
        public string Value
        {
            get => _value ?? throw new BadConfException($"The value is null.");
            set => _value = !string.IsNullOrWhiteSpace(value) ?
               value : throw new BadConfException($"The value can not be set as null or white space.");
        }

        /// <summary>
        /// Comment can be omitted.
        /// </summary>
        public string Comment
        {
            get => _comment ?? "";
            set => _comment = value;
        }
    }
}
