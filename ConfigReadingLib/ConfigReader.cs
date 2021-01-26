using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ConfigReadingLib.Member;

namespace ConfigReadingLib
{
    /// <summary>
    /// ConfigReader Class.
    /// Create ConfigReader's instance to read and write config file.
    /// Access method Create to create a new config file.
    /// </summary>
    public class ConfigReader
    {
        private string[] _rawConfigs;
        private readonly Dictionary<string, ConfigInfo> _configDic = new Dictionary<string, ConfigInfo>();
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
        /// If true, any meaningless line in config file will not be accepted.
        /// </param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="BadConfException"></exception>
        public ConfigReader(string path, string symbol = "//", bool strict = false)
        {
            _path = path;
            _symbol = symbol;
            _rawConfigs = File.ReadAllLines(_path);

            foreach (var line in _rawConfigs)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (LineInterpreter(line, out _) is ConfigInfo configinfo)
                {
                    if (!_configDic.ContainsKey(configinfo.Key))
                        _configDic.Add(configinfo.Key, configinfo);
                    else
                        throw new BadConfException("Key should be unique.");
                }
                else if (strict)
                {
                    throw new BadConfException("Config file is invalid.");
                }
            }
        }

        private ConfigInfo? LineInterpreter(string line, out string patterntext)
        {
            var comment = Regex.Match(line, $@"\s*({_symbol}.*)$").Groups[1].Value;
            var uncmtline = Regex.Replace(line, $@"(\s*){_symbol}.*$", "$1");
            var pairmatch = Regex.Match(uncmtline, @"^\s*(\S|\S.*?\S)\s*=\s*(\S|\S.*\S)\s*$");

            if (pairmatch.Success)
            {
                patterntext = Regex.Replace(uncmtline, @"^(\s*)(\S|\S.*?\S)(\s*)=(\s*)(\S|\S.*\S)(\s*)$", "$1#0$3=$4#1$6#2");
                return new ConfigInfo
                {
                    Key = pairmatch.Groups[1].Value,
                    Value = pairmatch.Groups[2].Value,
                    Comment = comment
                };
            }
            else
            {
                patterntext = line;
                return null;
            }
        }

        /// <summary>
        /// Get all keys existed in the config file.
        /// </summary>
        /// <returns>An array contained all keys existed in config file.</returns>
        public string[] GetAllKeys()
        {
            var keys = new List<string>();
            _configDic.ToList().ForEach(configinfo => keys.Add(configinfo.Key));
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
        {
            var result = _configDic.TryGetValue(key, out var configinfo);
            value = configinfo.Value;
            return result;
        }

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

            _configDic[key] = new ConfigInfo
            {
                Key = key,
                Value = value,
                Comment = _configDic[key].Comment
            };

            if (save) SaveChanges();
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
        /// <param name="configinfos">ConfigInfos.</param>
        /// <param name="overwrite">If true, the specified key which already existed will be overwritten.</param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void AddConfig(ConfigInfo[] configinfos, bool overwrite = false, bool save = true)
        {
            foreach (var configinfo in configinfos)
            {
                if (!_configDic.ContainsKey(configinfo.Key))
                    _configDic.Add(configinfo.Key, new ConfigInfo
                    {
                        Key = configinfo.Key,
                        Value = configinfo.Value,
                        Comment = $"{_symbol} {configinfo.Comment}"
                    });
                else if (overwrite)
                    _configDic[configinfo.Key] = configinfo;
                else
                    throw new BadConfException($"The specified key \"{configinfo.Key}\" already existed.");
            }

            if (save) SaveChanges();
        }

        /// <summary>
        /// Add a config.
        /// </summary>
        /// <param name="configinfo">ConfigInfo.</param>
        /// <param name="overwrite">If true, the specified key which already existed will be overwritten.</param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void AddConfig(ConfigInfo configinfo, bool overwrite = false, bool save = true)
            => AddConfig(new ConfigInfo[] { configinfo }, overwrite, save);

        /// <summary>
        /// Remove configs.
        /// </summary>
        /// <param name="keys">Keys to remove.</param>
        /// <param name="strict">If true, any key failed to remove will throw exception.</param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void RemoveConfig(string[] keys, bool strict = true, bool save = true)
        {
            foreach (var key in keys)
            {
                if (_configDic.ContainsKey(key))
                    _configDic.Remove(key);
                else if (strict)
                    throw new BadConfException($"The specified key \"{key}\" not found.");
            }

            if (save) SaveChanges();
        }

        /// <summary>
        /// Remove a config.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <param name="strict">If true, any key failed to remove will throw exception.</param>
        /// <param name="save">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void RemoveConfig(string key, bool strict = true, bool save = true)
            => RemoveConfig(new[] { key }, strict, save);

        private void Reflection(IMembers members, ParseFromString rule, string[] keys, bool exactmatch)
        {
            foreach (var key in keys)
            {
                var member = members.GetMember(key);
                if (member != null)
                    member.SetValue(rule[member.MemberType].Invoke(_configDic[key].Value));
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
            => Reflection(new PropertyMembers(obj), rule, GetAllKeys(), exactmatch);

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
            => Reflection(new PropertyMembers(obj), rule, keys, exactmatch);

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
            => Reflection(new FieldMembers(obj), rule, GetAllKeys(), exactmatch);

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
            => Reflection(new FieldMembers(obj), rule, keys, exactmatch);

        private void Reflection(IMembers members, ParseToString rule, string[] keys, bool exactmatch, bool save)
        {
            foreach (var key in keys)
            {
                var member = members.GetMember(key);
                if (member != null)
                {
                    var value = member.GetValue();
                    if (value != null)
                        _configDic[key] = new ConfigInfo
                        {
                            Key = key,
                            Value = rule[member.MemberType].Invoke(value),
                            Comment = _configDic[key].Comment
                        };
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
            => Reflection(new PropertyMembers(obj), rule, GetAllKeys(), exactmatch, save);

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
            => Reflection(new PropertyMembers(obj), rule, keys, exactmatch, save);

        /// <summary>
        /// Save all fields value contained within the class specified to the existent keys.
        /// </summary>
        /// <param name="obj">Class object.</param>
        /// <param name="rule">Parse rule.</param>
        /// <param name="strict">If true, any key failed to save from fields will throw exception.</param>
        /// <param name="exactmatch">If true, everything changed will be saved to the config file immediately.</param>
        /// <exception cref="BadConfException"></exception>
        public void SaveFieldsToKeys(object obj, ParseToString rule, bool strict = false, bool exactmatch = true)
            => Reflection(new FieldMembers(obj), rule, GetAllKeys(), strict, exactmatch);

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
            => Reflection(new FieldMembers(obj), rule, keys, exactmatch, save);

        /// <summary>
        /// Save configs to the config file.
        /// </summary>
        public void SaveChanges()
        {
            var rawconfigs = _rawConfigs.ToList();
            var restkeys = GetAllKeys().ToList();
            var deleteindex = new List<int>();

            for (var i = 0; i < _rawConfigs.Length; i++)
            {
                if (LineInterpreter(_rawConfigs[i], out var patterntext) is ConfigInfo configinfo)
                {
                    var key = configinfo.Key;
                    if (restkeys.Contains(key))
                    {
                        var blanks = Regex.Split(patterntext, @"#\d");
                        rawconfigs[i] = blanks[0] + key + blanks[1] + _configDic[key].Value + blanks[2] + _configDic[key].Comment;

                        restkeys.Remove(key);
                    }
                    else
                    {
                        deleteindex.Add(i);
                    }
                }
            }

            restkeys.ForEach(key =>
            {
                var configinfo = _configDic[key];
                rawconfigs.Add($"{configinfo.Key} = {configinfo.Value}  {configinfo.Comment}");
            });

            deleteindex.Reverse();
            deleteindex.ForEach(index => rawconfigs.RemoveAt(index));

            _rawConfigs = rawconfigs.ToArray();
            File.WriteAllLines(_path, _rawConfigs);
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
