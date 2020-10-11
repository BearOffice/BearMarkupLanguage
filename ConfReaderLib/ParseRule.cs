using System;
using System.Collections.Generic;

namespace ConfReaderLib
{
    public abstract class ParseRule<T1, T2>
    {
        private readonly Dictionary<Type, Func<T1, T2>> rules = new Dictionary<Type, Func<T1, T2>>();

        public Func<T1, T2> this[Type type]
        {
            get
            {
                if (rules.TryGetValue(type, out var func))
                    return func;
                else
                    throw new BadConfException("Rule not found.");
            }
            set
            {
                rules[type] = value;
            }
        }
    }

    /// <summary>
    /// Set the rules for parsing properties or fields' values to the config file.
    /// Default contains the rules of the type 'int' and 'string'.
    /// <para>Class ParseToString can be used like above:</para>
    /// <para>var rule = new ParseToString() { [typeof(int)] = x => x.ToString() };</para>
    /// </summary>
    public class ParseToString : ParseRule<object, string>
    {
        public ParseToString()
        {
            base[typeof(int)] = x => x.ToString();
            base[typeof(string)] = x => x.ToString();
        }
    }

    /// <summary>
    /// Set the rules for parsing the config file to properties or fields' values.
    /// Default contains the rules of the type 'int' and 'string'.
    /// <para>Class ParseToString can be used like above:</para>
    /// <para>var rule = new ParseFromString() { [typeof(int)] = x => int.Parse(x) };</para>
    /// </summary>
    public class ParseFromString : ParseRule<string, object>
    {
        public ParseFromString()
        {
            base[typeof(int)] = x => int.Parse(x);
            base[typeof(string)] = x => x;
        }
    }
}
