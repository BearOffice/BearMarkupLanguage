using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ConfigReadingLib
{
    /// <summary>
    /// ConfigReader Class.
    /// Create ConfigReader's instance to read and write config file.
    /// Access method Create to create a new config file.
    /// </summary>
    public class ConfigReader
    {
        private readonly List<string> _rawConfig;
        private readonly Dictionary<string, string> _configDic = new Dictionary<string, string>();
        private readonly string _path;
        private readonly string _symbol;

        /// <summary>
        /// Read Config File.
        /// Config file's format should be like as above:
        /// <para>A key = Value A  {commentout symbol} Comment</para>
        /// Key and value will be separated with the first '='.
        /// Blank space around '=' will be ignored.
        /// <para>Key and value that include blank space is acceptable.
        /// Key and value are case sensitive.
        /// Key should be unique.</para>
        /// </summary>
        /// <param name="path">The path Specified is exist.</param>
        /// <param name="symbol">Comment out symbol to set. Default is '//'.</param>
        /// <param name="strict">
        /// If true, the config file's format will be checked strictly.
        /// Any meaningless chars will not be accepted.
        /// </param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="BadConfException"></exception>
        public ConfigReader(string path, string symbol = "//", bool strict = false)
        {
            _path = path;
            _symbol = symbol;
            _rawConfig = File.ReadAllLines(_path).ToList();

            foreach (var line in _rawConfig)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (LineInterpreter(line, out _) is ConfigInfo configinfo)
                {
                    if (!_configDic.ContainsKey(configinfo.Key))
                        _configDic.Add(configinfo.Key, configinfo.Value);
                    else
                        throw new BadConfException("Key should be unique.");
                }
                else if (strict)
                {
                    throw new BadConfException("Config file is invalid.");
                }
            }
        }

        private ConfigInfo? LineInterpreter(string line, out (string left, string right) rawline)
        {
            var comment = Regex.Match(line, $@"\s*{_symbol}.*").Value;

            var uncommentline = Regex.Replace(line, comment, "");
            var left = Regex.Match(uncommentline, @"(\S|\S.*?\S)\s*=");
            var right = Regex.Match(uncommentline, @"=\s*(\S.*\S|\S)");

            rawline = (left.Value, right.Value);

            if (left.Success && right.Success)
            {
                return new ConfigInfo
                {
                    Key = left.Groups[1].Value,
                    Value = right.Groups[1].Value,
                    Comment = comment
                };
            }
            else
            {
                return null;
            }
        }

        private void Reflection(IMember members, ParseFromString rule, string[] keys, bool exactmatch)
        {
            foreach (var key in keys)
            {
                if (members.HasMember(key))
                    members.SetValue(key, rule[members.GetMemberType(key)].Invoke(GetValue(key)));
                else if (exactmatch)
                    throw new BadConfException($"The specified key \"{key}\" not found.");
            }
        }

        /// <summary>
        /// Set all properties value contained within the class specified from the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="exactmatch">
        /// If true, any property failed to set value from the keys will throw exception.
        /// </param>
        /// <exception cref="BadConfException"></exception>
        public void SetPropertiesFromKeys(object obj, ParseFromString rule, bool exactmatch = false)
            => Reflection(new PropertyMember(obj), rule, GetAllKeys(), exactmatch);

        /// <summary>
        /// Set properties value contained within the class specified from the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="keys">
        /// Specify the keys that need to be set. 
        /// </param>
        /// <param name="exactmatch">
        /// If true, any property failed to set value from the keys will throw exception.
        /// </param>
        /// <exception cref="BadConfException"></exception>
        public void SetPropertiesFromKeys(object obj, ParseFromString rule, string[] keys, bool exactmatch = false)
            => Reflection(new PropertyMember(obj), rule, keys, exactmatch);

        /// <summary>
        /// Set all fields value contained within the class specified from the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="exactmatch">
        /// If true, any field failed to set value from the keys will throw exception.
        /// </param>
        /// <exception cref="BadConfException"></exception>
        public void SetFieldsFromKeys(object obj, ParseFromString rule, bool exactmatch = false)
            => Reflection(new FieldMember(obj), rule, GetAllKeys(), exactmatch);

        /// <summary>
        /// Set fields value contained within the class specified from the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="keys">
        /// Specify the keys that need to be set.
        /// </param>
        /// <param name="exactmatch">
        /// If true, any field failed to set value from the keys will throw exception.
        /// </param>
        /// <exception cref="BadConfException"></exception>
        public void SetFieldsFromKeys(object obj, ParseFromString rule, string[] keys = null, bool exactmatch = false)
            => Reflection(new FieldMember(obj), rule, keys, exactmatch);

        /// <summary>
        /// Get all keys existed in the config file.
        /// </summary>
        /// <returns>An array contained all keys existed in config file.</returns>
        public string[] GetAllKeys()
        {
            var keys = new List<string>();
            _configDic.ToList().ForEach(pair => keys.Add(pair.Key));
            return keys.ToArray();
        }

        /// <summary>
        /// Determine whether the config file contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if config contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key)
            => _configDic.ContainsKey(key);

        /// <summary>
        /// Try to get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if config contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out string value)
            => _configDic.TryGetValue(key, out value);

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
                throw new BadConfException($"The specified key \"{key}\" not found.");
        }

        /// <summary>
        /// Try to change the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <returns>true if the value the specified key changed successfully; otherwise, false.</returns>
        public bool TryChangeValue(string key, string value, bool save = true)
        {
            if (!_configDic.ContainsKey(key))
                return false;

            _configDic[key] = value;
            if (save)
                SaveChanges();
            return true;
        }

        /// <summary>
        /// Change the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void ChangeValue(string key, string value, bool save = true)
        {
            if (!TryChangeValue(key, value, save))
                throw new BadConfException($"The specified key \"{key}\" not found.");
        }

        /// <summary>
        /// Add configs.
        /// </summary>
        /// <param name="configInfos">ConfigInfos.</param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void AddConfig(ConfigInfo[] configInfos, bool save = true)
        {
            foreach (var conf in configInfos)
            {
                if (!_configDic.ContainsKey(conf.Key))
                    _configDic.Add(conf.Key, conf.Value);
                else
                    throw new BadConfException($"The specified key \"{conf.Key}\" already exist.");

                var rawline = $"{conf.Key} = {conf.Value}";
                rawline += (conf.Comment == "") ? "" : $"  {_symbol} {conf.Comment}";

                _rawConfig.Add(rawline);
            }

            if (save) SaveChanges();
        }

        private void Reflection(IMember members, ParseToString rule, string[] keys, bool exactmatch, bool save)
        {
            foreach (var key in keys)
            {
                if (members.HasMember(key))
                {
                    var value = members.GetValue(key);
                    if (value != null)
                        ChangeValue(key, rule[members.GetMemberType(key)].Invoke(value), save: false);
                    else if (exactmatch)
                        throw new BadConfException("The field / property in the provided object is null.");
                }
                else if (exactmatch)
                {
                    throw new BadConfException($"The specified key \"{key}\" not found.");
                }
            }

            if (save) SaveChanges();
        }

        /// <summary>
        /// Save all properties value contained within the class specified to the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="exactmatch">If true, any key failed to save from the properties will throw exception.</param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void SavePropertiesToKeys(object obj, ParseToString rule, bool exactmatch = false, bool save = true)
            => Reflection(new PropertyMember(obj), rule, GetAllKeys(), exactmatch, save);

        /// <summary>
        /// Save properties value contained within the class specified to the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="keys">Specify the keys that need to be save. </param>
        /// <param name="exactmatch">If true, any key failed to save from the properties will throw exception.</param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void SavePropertiesToKeys(object obj, ParseToString rule, string[] keys,
            bool exactmatch = false, bool save = true)
            => Reflection(new PropertyMember(obj), rule, keys, exactmatch, save);

        /// <summary>
        /// Save all fields value contained within the class specified to the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="strict">If true, any key failed to save from fields will throw exception.</param>
        /// <param name="exactmatch">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void SaveFieldsToKeys(object obj, ParseToString rule, bool strict = false, bool exactmatch = true)
            => Reflection(new FieldMember(obj), rule, GetAllKeys(), strict, exactmatch);

        /// <summary>
        /// Save fields value contained within the class specified to the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="keys">Specify the keys that need to be save. </param>
        /// <param name="exactmatch">If true, any key failed to save from the fields will throw exception.</param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void SaveFieldsToKeys(object obj, ParseToString rule, string[] keys, bool exactmatch = false, bool save = true)
            => Reflection(new FieldMember(obj), rule, keys, exactmatch, save);

        /// <summary>
        /// Save configs to the config file.
        /// </summary>
        public void SaveChanges()
        {
            int i = 0;
            foreach (var line in _rawConfig.ToArray())
            {
                if (LineInterpreter(line, out var rawline) is ConfigInfo configinfo)
                {
                    if (_configDic.ContainsKey(configinfo.Key))
                    {
                        var newright = rawline.right.Replace(configinfo.Value, _configDic[configinfo.Key]).Remove(0, 1); // Remove '='
                        _rawConfig[i] = rawline.left + newright + configinfo.Comment;
                    }
                }
                i++;
            }
            File.WriteAllLines(_path, _rawConfig);
        }

        /// <summary>
        /// Create a new config file, write the specified configs. If the file specified is exist, it will be overwritten.
        /// </summary>
        /// <param name="configinfos">ConfigInfos.</param>
        /// <param name="path">The path Specified is exist.</param>
        /// <param name="symbol">Comment out symbol to set. Default is '//'.</param>
        /// <exception cref="BadConfException"></exception>
        public static void Create(ConfigInfo[] configinfos, string path, string symbol = "//")
        {
            File.WriteAllText(path, "");
            var reader = new ConfigReader(path, symbol);
            reader.AddConfig(configinfos);
        }
    }
}
