using System;
using System.Collections.Generic;

namespace ConfigReadingLib
{
    /// <summary>
    /// ParseRule.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public abstract class ParseRule<T1, T2>
    {
        private readonly Dictionary<Type, Func<T1, T2>> rules = new Dictionary<Type, Func<T1, T2>>();

        /// <summary>
        /// Parse rule.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Func<T1, T2> this[Type type]
        {
            get
            {
                if (rules.TryGetValue(type, out var func))
                    return func;
                else
                    throw new BadConfException($"Parse rule for type '{type}' not found.");
            }
            set
            {
                rules[type] = value;
            }
        }
    }

    /// <summary>
    /// ParseToString.
    /// </summary>
    public class ParseToString : ParseRule<object, string>
    {
        /// <summary>
        /// Set the rules for parsing properties or fields' values to the config file.
        /// Default contains the following rules: type 'int', 'string'.
        /// <para>Class ParseToString can be used like above:</para>
        /// <para>var rule = new ParseToString() { [typeof(int)] = x => x.ToString() };</para>
        /// </summary>
        public ParseToString()
        {
            base[typeof(int)] = x => x.ToString();
            base[typeof(string)] = x => x.ToString();
        }
    }

    /// <summary>
    /// ParseFromString.
    /// </summary>
    public class ParseFromString : ParseRule<string, object>
    {
        /// <summary>
        /// Set the rules for parsing the config file to properties or fields' values.
        /// Default contains the following rules: type 'int', 'string'.
        /// <para>Class ParseToString can be used like above:</para>
        /// <para>var rule = new ParseFromString() { [typeof(int)] = x => int.Parse(x) };</para>
        /// </summary>
        public ParseFromString()
        {
            base[typeof(int)] = x => int.Parse(x);
            base[typeof(string)] = x => x;
        }
    }
}