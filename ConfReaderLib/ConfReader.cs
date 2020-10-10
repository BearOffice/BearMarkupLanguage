using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        /// Any meaningless chars won't be accepted.
        /// </param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="BadConfException"></exception>
        public ConfReader(string path, string symbol = "//", bool strict = false)
        {
            this._path = path;
            this._symbol = symbol;
            this._strict = strict;

            _rawconf = File.ReadAllLines(_path).ToList();

            foreach (var line in _rawconf)
            {
                if (LineInterpreter(line, out var pair, out _, out bool isblank))
                {
                    if (!_pairs.ContainsKey(pair.key))
                        _pairs.Add(pair.key, pair.value);
                    else
                        throw new BadConfException("Key should be unique.");
                }
                else if (!isblank && _strict)
                {
                    throw new BadConfException("Config file is invalid.");
                }
            }
        }

        private bool LineInterpreter(string line, out (string key, string value, string comment) pair,
            out (string left, string right) rawline, out bool isBlank)
        {
            var comment = Regex.Match(line, $@"\s*{_symbol}.*").Value;
            var uncommentline = Regex.Replace(line, comment, "");
            var left = Regex.Match(uncommentline, @"(\S|\S.*?\S)\s*=");
            var right = Regex.Match(uncommentline, @"=\s*(\S.*\S|\S)");
            rawline = (left.Value, right.Value);
            if (left.Success && right.Success)
            {
                var key = left.Groups[1].Value;
                var value = right.Groups[1].Value;
                pair = (key, value, comment);
                isBlank = false;
                return true;
            }
            else
            {
                pair = (null, null, comment);
                if (Regex.Match(uncommentline, @"\s*").Success)
                    isBlank = true;
                else
                    isBlank = false;
                return false;
            }
        }

        /// <summary>
        /// Get all keys existed in config.
        /// </summary>
        /// <returns>An array contained all keys existed in config.</returns>
        public string[] GetAllKeys()
        {
            var keys = new List<string>();
            _pairs.ToList().ForEach(pair => keys.Add(pair.Key));
            return keys.ToArray();
        }

        /// <summary>
        /// Determine whether config contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if config contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key)
            => _pairs.ContainsKey(key);

        /// <summary>
        /// Try to get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if config contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out string value)
            => _pairs.TryGetValue(key, out value);

        /// <summary>
        /// Get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>An element contained with the specified key.</returns>
        /// <exception cref="BadConfException"></exception>
        public string GetValue(string key)
        {
            if (TryGetValue(key, out string value))
                return value;
            else
                throw new BadConfException("The specified key not found.");
        }

        /// <summary>
        /// Try to change the config. Default: Save configs to the config file.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="save">If true, update the config file immediately.</param>
        /// <returns>true if the value the specified key changed successfully; otherwise, false.</returns>
        public bool TryChangeValue(string key, string value, bool save = true)
        {
            if (!_pairs.ContainsKey(key))
                return false;

            _pairs[key] = value;
            if (save)
                SaveConf();
            return true;
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
            if (!TryChangeValue(key, value, save))
                throw new BadConfException($"The key \"{key}\" does not exist.");
        }

        /// <summary>
        /// Add configs. Default: Save configs to the config file.
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="save">If true, update the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void AddConf((string key, string value, string comment)[] pairs, bool save = true)
        {
            foreach (var (key, value, comment) in pairs)
            {
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
                if (LineInterpreter(line, out var pair, out var rawline, out _))
                {
                    if (_pairs.ContainsKey(pair.key))
                    {
                        var newright = rawline.right.Replace(pair.value, _pairs[pair.key]).Remove(0, 1); // Remove '='
                        _rawconf[i] = rawline.left + newright + pair.comment;
                    }
                }
                i++;
            }
            File.WriteAllLines(_path, _rawconf);
        }

        /// <summary>
        /// Create a new conf file. If the file specified is exist, it will be overwritten.
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="path"></param>
        /// <param name="symbol"></param>
        /// <exception cref="BadConfException"></exception>
        public static void Create((string key, string value, string comment)[] pairs, string path, string symbol = "//")
        {
            File.WriteAllText(path, "");
            var reader = new ConfReader(path, symbol);
            reader.AddConf(pairs);
        }
    }
}
