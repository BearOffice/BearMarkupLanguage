using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace ConfReaderLib
{
    /// <summary>
    /// ConfReader
    /// </summary>
    public class ConfReader
    {
        private readonly List<string> _rawconf;
        private readonly Dictionary<string, string> _pairs = new Dictionary<string, string>();
        private readonly string _path;
        private readonly string _symbol;
        private readonly bool _strict;
        /// <summary>
        /// Read Config File.
        /// Config file's format should be like as above:
        /// <para>A key = Value A  {commentout symbol} Comment</para>
        /// Key and value will be separated with the first '='.
        /// Blank space around '=' will be ignored.
        /// <para>Key and value which include blank space is acceptable.
        /// Key and value are case sensitive.
        /// Key should be unique.</para>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="symbol"></param>
        /// <param name="strict">
        /// If ture, check the config file's format strictly.
        /// Any meaningless chars won't be accepted
        /// </param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="BadConfException"></exception>
        public ConfReader(string path, string symbol = "//", bool strict = false)
        {
            this._path = path;
            this._symbol = symbol;
            this._strict = strict;
            // Try read file
            try
            {
                _rawconf = File.ReadAllLines(_path).ToList();
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"The config file which located in \"{_path}\" does not exist.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            foreach (var line in _rawconf)
            {
                var uncommentline = Regex.Replace(line, $@"\s*{_symbol}.*", "");
                var left = Regex.Match(uncommentline, @"(\S|\S.*?\S)\s*=");
                var right = Regex.Match(uncommentline, @"=\s*(\S.*\S|\S)");
                if (left.Success && right.Success)
                {
                    var key = left.Groups[1].Value;
                    var value = right.Groups[1].Value;
                    if (!_pairs.TryGetValue(key, out _))
                        _pairs.Add(key, value);
                    else
                        throw new BadConfException("Key should be unique.");
                }
                else if (uncommentline != "" && _strict)
                {
                    throw new BadConfException("Config file is invalid.");
                }
            }

            if (_pairs.ContainsValue(null))
                throw new BadConfException("Some keys or values are not matched in config file.");
        }

        /// <summary>
        /// Get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>An element contained with the specified key.</returns>
        /// <exception cref="BadConfException"></exception>
        public string GetValue(string key)
        {
            if (_pairs.TryGetValue(key, out string value))
                return value;
            else
                throw new BadConfException("The specified key not found.");
        }

        /// <summary>
        /// Try to get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if config contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out string value)
        {
            if (_pairs.TryGetValue(key, out value))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Change the config. Default: Save configs to the config file.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="save">If true, update the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void ChangeValue(string key, string value, bool save = true)
        {
            if (_pairs.ContainsKey(key))
                _pairs[key] = value;
            else
                throw new BadConfException($"The key \"{key}\" does not exist.");

            if (save)
                SaveConf();
        }

        /// <summary>
        /// Add a config. Default: Save configs to the config file.
        /// </summary>
        /// <param name="sets">
        /// Set contains key, value and comment splitted by ','. Set should be formatted as below:
        /// <para>"Key A,Value A,Comment A"</para>
        /// Comment is optional.
        /// </param>
        /// <param name="save"></param>
        public void AddConf(string[] sets, bool save = true)
        {
            foreach (var set in sets)
            {
                var key = "";
                var value = "";
                var comment = "";
                var kvc = set.Split(',');
                if (kvc.Length == 2)
                {
                    key = kvc[0];
                    value = kvc[1];
                }
                else if (kvc.Length == 3)
                {
                    key = kvc[0];
                    value = kvc[1];
                    comment = kvc[2];
                }
                else
                {
                    throw new BadConfException("The sets' format is invalid.");
                }

                if (!_pairs.ContainsKey(key))
                    _pairs.Add(key, value);
                else
                    throw new BadConfException($"The key \"{key}\" already exist.");

                if (comment == "")
                    _rawconf.Add($"{key} = {value}");
                else
                    _rawconf.Add($"{key} = {value}  {_symbol} {comment}");
            }

            if (save)
                SaveConf();
        }

        /// <summary>
        /// Save configs to the config file immediately.
        /// </summary>
        public void SaveConf()
        {
            int i = 0;
            foreach (var line in _rawconf.ToArray())
            {
                var comment = Regex.Match(line, $@"\s*{_symbol}.*").Value;
                var uncommentline = Regex.Replace(line, comment, "");
                var left = Regex.Match(uncommentline, @"(\S|\S.*?\S)\s*=");
                var right = Regex.Match(uncommentline, @"=\s*(\S.*\S|\S)");

                if (left.Success && right.Success)
                {
                    var key = left.Groups[1].Value;
                    if (_pairs.ContainsKey(key))
                    {
                        var newright = right.Value.Replace(right.Groups[1].Value, _pairs[key]).Remove(0, 1); // Remove '='
                        _rawconf[i] = left.Value + newright + comment;
                    }
                }

                i++;
            }

            File.WriteAllLines(_path, _rawconf);
        }

        /// <summary>
        /// Create a new conf file. If the file specified is exist, it will be overwritten.
        /// </summary>
        /// <param name="sets">
        /// Set contains key, value and comment splitted by ','. Set should be formatted as below:
        /// <para>"Key A,Value A,Comment A"</para>
        /// Comment is optional.
        /// </param>
        /// <param name="path"></param>
        /// <param name="symbol"></param>
        /// <exception cref="BadConfException"></exception>
        public static void Create(string[] sets, string path, string symbol = "//")
        {
            File.WriteAllText(path, "");
            var reader = new ConfReader(path, symbol);
            reader.AddConf(sets);
        }
    }
}
