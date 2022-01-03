using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMLLib.Serialization.Conversion;
using BearMLLib.Serialization;
using BearMLLib.Core;
using BearMLLib.Configs;
using BearMLLib.Core.Helpers;
using BearMLLib.Text;

namespace BearMLLib
{
    /// <summary>
    /// Get or save configs.
    /// For more information: <a href="https://github.com/BearOffice/BearMarkupLanguageLib">github.com/BearOffice/</a>
    /// </summary>
    public class BearML
    {
        private readonly ConfigsContainer _container;
        private readonly Writer _writer;
        private readonly IConversionProvider[] _providers;
        private readonly bool _autoSave;

        /// <summary>
        /// Create a blank configs container.
        /// </summary>
        public BearML()
        {
            _container = new Reader().GenerateConfigs();
            _writer = default;
            _providers = Array.Empty<IConversionProvider>();
            _autoSave = false;

            ErrorHandler.This.Message += ThrowError;
        }

        /// <summary>
        /// Create a blank configs container.
        /// </summary>
        /// <param name="providers">Provide the methods to convert specified types.</param>
        public BearML(IConversionProvider[] providers)
        {
            _container = new Reader().GenerateConfigs();
            _writer = default;
            _providers = providers;
            _autoSave = false;

            ErrorHandler.This.Message += ThrowError;
        }

        /// <summary>
        /// Create a configs container from raw.
        /// </summary>
        /// <param name="raw">Raw lines.</param>
        public BearML(string[] raw)
        {
            _container = new Reader(raw).GenerateConfigs();
            _writer = default;
            _providers = Array.Empty<IConversionProvider>();
            _autoSave = false;

            ErrorHandler.This.Message += ThrowError;
        }

        /// <summary>
        /// Create a configs container from raw.
        /// </summary>
        /// <param name="raw">Raw lines.</param>
        /// <param name="providers">Provide the methods to convert specified types.</param>
        public BearML(string[] raw, IConversionProvider[] providers)
        {
            _container = new Reader(raw).GenerateConfigs();
            _writer = default;
            _providers = providers;
            _autoSave = false;

            ErrorHandler.This.Message += ThrowError;
        }

        /// <summary>
        /// Get or save configs.
        /// </summary>
        /// <param name="path">The path of the config file.</param>
        /// <param name="autoSave">if <see langword="true" />, save the changes automatically.</param>
        public BearML(string path, bool autoSave = true)
        {
            _container = new Reader(path).GenerateConfigs();
            _writer = new Writer(path);
            _providers = Array.Empty<IConversionProvider>();
            _autoSave = autoSave;

            ErrorHandler.This.Message += ThrowError;
        }

        /// <summary>
        /// Get or save configs.
        /// </summary>
        /// <param name="path">The path of the config file</param>
        /// <param name="providers">Provide the methods to convert specified types.</param>
        /// <param name="autoSave">if <see langword="true" />, save the changes automatically.</param>
        public BearML(string path, IConversionProvider[] providers, bool autoSave = true)
        {
            _container = new Reader(path).GenerateConfigs();
            _writer = new Writer(path);
            _providers = providers;
            _autoSave = autoSave;

            ErrorHandler.This.Message += ThrowError;
        }

        /// <summary>
        /// Get content from key in default group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="key">Key name or key alias name.</param>
        /// <returns>Content with the specified type.</returns>
        public T GetContent<T>(string key)
        {
            return GetContent<T>(Identifier.DefaultGroupName, key);
        }

        /// <summary>
        /// Get content from key in the specified group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <returns>Content with the specified type.</returns>
        public T GetContent<T>(string group, string key)
        {
            if (!TryGetContent<T>(group, key, out var content))
                ErrorHandler.This.Add(ErrorType.NotMatch, $"{group}.{key}");

            return content;
        }

        /// <summary>
        /// Try get content from key in default group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="content">Content with the specified type.</param>
        /// <returns><see langword="true"/> if group and key exist; otherwise, <see langword="false"/>.</returns>
        public bool TryGetContent<T>(string key, out T content)
        {
            return TryGetContent(Identifier.DefaultGroupName, key, out content);
        }

        /// <summary>
        /// Try get content from key in the specified group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="content">Content with the specified type.</param>
        /// <returns><see langword="true"/> if group and key exist; otherwise, <see langword="false"/>.</returns>
        public bool TryGetContent<T>(string group, string key, out T content)
        {
            content = default;
            if (!TryGetKeyName(group, key, out var keyName)) return false;
            
            content = _container.GroupsDic[group].KeyContentPairsDic[keyName].Content.Get<T>(_providers);
            return true;
        }

        /// <summary>
        /// Change content binded by key in default group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="content">Content with the specified type.</param>
        public void ChangeContent<T>(string key, T content)
        {
            ChangeContent(Identifier.DefaultGroupName, key, content);
        }

        /// <summary>
        /// Change content binded by key in the specified group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="content">Content with the specified type.</param>
        public void ChangeContent<T>(string group, string key, T content)
        {
            var keyName = GetKeyName(group, key);
            var contentObj = ContentTypeSelector.GetContent(content, _providers);

            _container.GroupsDic[group].KeyContentPairsDic[keyName].Content = contentObj;
            AutoSave();
        }

        /// <summary>
        /// Get comment from key in default group.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        /// <returns>Comment</returns>
        public string GetComment(string key)
        {
            return GetComment(Identifier.DefaultGroupName, key);
        }

        /// <summary>
        /// Get comment from key in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <returns>Comment.</returns>
        public string GetComment(string group, string key)
        {
            if (!TryGetComment(group, key, out var comment))
                ErrorHandler.This.Add(ErrorType.NotMatch, $"{group}.{key}");
            
            return comment;
        }

        /// <summary>
        /// Try get comment from key in default group.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="comment">Comment.</param>
        /// <returns><see langword="true"/> if group and key exist; otherwise, <see langword="false"/>.</returns>
        public bool TryGetComment(string key, out string comment)
        {
            return TryGetComment(Identifier.DefaultGroupName, key, out comment);
        }

        /// <summary>
        /// Try get comment from key in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="comment">Comment.</param>
        /// <returns><see langword="true"/> if group and key exist; otherwise, <see langword="false"/>.</returns>
        public bool TryGetComment(string group, string key, out string comment)
        {
            comment = default;
            if (!TryGetKeyName(group, key, out var keyName)) return false;

            comment = _container.GroupsDic[group].KeyContentPairsDic[keyName].Key.Comment;
            return true;
        }

        /// <summary>
        /// Change comment binded by key in default group.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="comment">Comment.</param>
        public void ChangeComment(string key, string comment)
        {
            ChangeComment(Identifier.DefaultGroupName, key, comment);
        }

        /// <summary>
        /// Change comment binded by key in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="comment">Comment.</param>
        public void ChangeComment(string group, string key, string comment)
        {
            var keyName = GetKeyName(group, key);

            var currentKey = _container.GroupsDic[group].KeyContentPairsDic[keyName].Key;
            _container.GroupsDic[group].KeyContentPairsDic[keyName].Key =
                new Key(currentKey.Name, currentKey.AliasName, comment);

            AutoSave();
        }

        /// <summary>
        /// Get key alias from key in default group.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        /// <returns>Key alias.</returns>
        public string GetKeyAlias(string key)
        {
            return GetKeyAlias(Identifier.DefaultGroupName, key);
        }

        /// <summary>
        /// Get key alias from key in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <returns>Key alias.</returns>
        public string GetKeyAlias(string group, string key)
        {
            if (!TryGetKeyAlias(group, key, out var keyAlias))
                ErrorHandler.This.Add(ErrorType.NotMatch, $"{group}.{key}");

            return keyAlias;
        }

        /// <summary>
        /// Try get key alias from key in default group.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="keyAlias">Key alias.</param>
        /// <returns><see langword="true"/> if group and key exist; otherwise, <see langword="false"/>.</returns>
        public bool TryGetKeyAlias(string key, out string keyAlias)
        {
            return TryGetKeyAlias(Identifier.DefaultGroupName, key, out keyAlias);
        }

        /// <summary>
        /// Try get key alias from key in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="keyAlias">Key alias.</param>
        /// <returns><see langword="true"/> if group and key exist; otherwise, <see langword="false"/>.</returns>
        public bool TryGetKeyAlias(string group, string key, out string keyAlias)
        {
            keyAlias = default;
            if (!TryGetKeyName(group, key, out var keyName)) return false;

            keyAlias = _container.GroupsDic[group].KeyContentPairsDic[keyName].Key.AliasName;
            return true;
        }

        /// <summary>
        /// Change key alias binded by key in default group.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="keyAlias">Key alias. Key alias must be formatted as below.
        /// <code>"^@?[a-zA-Z_][a-zA-Z_0-9]*$"</code></param>
        public void ChangeKeyAlias(string key, string keyAlias)
        {
            ChangeKeyAlias(Identifier.DefaultGroupName, key, keyAlias);
        }

        /// <summary>
        /// Change key alias binded by key in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <param name="keyAlias">Key alias.</param>
        public void ChangeKeyAlias(string group, string key, string keyAlias)
        {
            var keyName = GetKeyName(group, key);

            var currentKey = _container.GroupsDic[group].KeyContentPairsDic[keyName].Key;
            _container.GroupsDic[group].KeyContentPairsDic[keyName].Key =
                new Key(currentKey.Name, keyAlias, currentKey.Comment);

            AutoSave();
        }

        /// <summary>
        /// Get all keys in default group.
        /// </summary>
        public string[] GetAllKeys()
        {
            return GetAllKeys(Identifier.DefaultGroupName);
        }

        /// <summary>
        /// Get all keys in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        public string[] GetAllKeys(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch, group);

            return _container.GroupsDic[group].KeyContentPairsDic.Keys.ToArray();
        }

        /// <summary>
        /// Get all groups.
        /// </summary>
        public string[] GetAllGroups()
        {
            return _container.GroupsDic.Keys.ToArray();
        }

        /// <summary>
        /// Determines whether default group contains the specified key.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        /// <returns><see langword="true"/> if group and key exist; otherwise, <see langword="false"/></returns>
        public bool ContainsKey(string key)
        {
            return ContainsKey(Identifier.DefaultGroupName, key);
        }

        /// <summary>
        /// Determines whether the specified group contains the specified key.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        /// <returns><see langword="true"/> if group and key exist; otherwise, <see langword="false"/></returns>
        public bool ContainsKey(string group, string key)
        {
            return TryGetKeyName(group, key, out _);
        }

        /// <summary>
        /// Determines whether the specified group contains the specified key.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <returns><see langword="true"/> if group exists; otherwise, <see langword="false"/></returns>
        public bool ContainsGroup(string group)
        {
            return _container.GroupsDic.ContainsKey(group);
        }

        /// <summary>
        /// Add key to default group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="key">Key name.</param>
        /// <param name="content">Content.</param>
        public void AddKey<T>(string key, T content)
        {
            AddKey(Identifier.DefaultGroupName, key, null, null, content);
        }

        /// <summary>
        /// Add key to the specified group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name.</param>
        /// <param name="content">Content.</param>
        public void AddKey<T>(string group, string key, T content)
        {
            AddKey(group, key, null, null, content);
        }

        /// <summary>
        /// Add key to default group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="key">Key name.</param>
        /// <param name="keyAlias">Key alias name. Set <see langword="null"/> if not necessary.</param>
        /// <param name="comment">Comment. Set <see langword="null"/> if not necessary.</param>
        /// <param name="content">Content.</param>
        public void AddKey<T>(string key, string keyAlias, string comment, T content)
        {
            AddKey(Identifier.DefaultGroupName, key, keyAlias, comment, content);
        }

        /// <summary>
        /// Add key to the specified group.
        /// </summary>
        /// <typeparam name="T">Content type.</typeparam>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name.</param>
        /// <param name="keyAlias">Key alias name. Set <see langword="null"/> if not necessary.</param>
        /// <param name="comment">Comment. Set <see langword="null"/> if not necessary.</param>
        /// <param name="content">Content.</param>
        public void AddKey<T>(string group, string key, string keyAlias, string comment, T content)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch, group);

            if (_container.GroupsDic[group].KeyContentPairsDic.ContainsKey(key))
                ErrorHandler.This.Add(ErrorType.InvalidConfig, "Key already exists.");

            var keyObj = new Key(key, keyAlias, comment);
            var contentObj = ContentTypeSelector.GetContent(content, _providers);
            var pair = new KeyContentPair(keyObj, contentObj);

            _container.GroupsDic[group].KeyContentPairsDic.Add(key, pair);
            _container.GroupsDic[group].OrderedLine.Add(new TaggedLine(true, key));
            AutoSave();
        }

        /// <summary>
        /// Add an empty group.
        /// </summary>
        /// <param name="group">Group name.</param>
        public void AddEmptyGroup(string group)
        {
            if (ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.InvalidConfig, "Group already exists.");

            _container.GroupsDic.Add(group, new Group(group));
            // at an empty line
            // _container.GroupsDic[group].OrderedLine.Add(new TaggedLine(false, string.Empty));
            _container.OrderedLine.Add(new TaggedLine(true, group));
            AutoSave();
        }

        /// <summary>
        /// Get raw text from key in default group.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        public string[] GetKeyContentPairRawText(string key)
        {
            return GetKeyContentPairRawText(Identifier.DefaultGroupName, key);
        }

        /// <summary>
        /// Get raw text from key in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        public string[] GetKeyContentPairRawText(string group, string key)
        {
            if (!TryGetKeyName(group, key, out var keyName))
                ErrorHandler.This.Add(ErrorType.NotMatch, $"{group}.{key}");

            var raw = _container.GroupsDic[group].KeyContentPairsDic[keyName].Raw;

            return raw.ToArray();
        }

        /// <summary>
        /// Get default group's raw text.
        /// </summary>
        public string[] GetGroupRawText()
        {
            return GetGroupRawText(Identifier.DefaultGroupName);
        }

        /// <summary>
        /// Get the specified group's raw text.
        /// </summary>
        /// <param name="group">Group name.</param>
        public string[] GetGroupRawText(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch, group);

            var raw = GroupParser.ParseToRaw(_container.GroupsDic[group]);

            return raw.ToArray();
        }

        /// <summary>
        /// Deserialize the specified group to object.
        /// </summary>
        /// <typeparam name="T">Object Type.</typeparam>
        /// <param name="group"></param>
        /// <returns>Object with the specified type.</returns>
        public T DeserializeObjectGroup<T>(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch, group);

            var pairs = _container.GroupsDic[group].KeyContentPairsDic.Values.ToArray();

            return ObjectSerializer.Deserialize<T>(pairs, _providers);
        }

        /// <summary>
        /// Seralize the specified object to a new group.
        /// </summary>
        /// <typeparam name="T">Object Type.</typeparam>
        /// <param name="group"></param>
        /// <param name="source">Source object.</param>
        /// <returns>Object with the specified type.</returns>
        public void AddObjectGroup<T>(string group, T source)
        {
            if (ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.InvalidConfig, "Group already exists.");

            _container.GroupsDic.Add(group, new Group(group));
            // at an empty line
            // _container.OrderedLine.Add(new TaggedLine(false, string.Empty));
            _container.OrderedLine.Add(new TaggedLine(true, group));

            var pairs = ObjectSerializer.Serialize(source, _providers);

            foreach (var pair in pairs)
            {
                _container.GroupsDic[group].KeyContentPairsDic.Add(pair.Key.Name, pair);
                _container.GroupsDic[group].OrderedLine.Add(new TaggedLine(true, pair.Key.Name));
            }

            AutoSave();
        }

        /// <summary>
        /// Change the group with the result of the serialized object specified.
        /// </summary>
        /// <typeparam name="T">Object Type.</typeparam>
        /// <param name="group"></param>
        /// <param name="source">Source object.</param>
        public void ChangeObjectGroup<T>(string group, T source)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch, group);

            var pairs = ObjectSerializer.Serialize(source, _providers);

            if ( _container.GroupsDic[group].KeyContentPairsDic.Count != pairs.Length)
                throw new InvalidConfigException("The given object is inconsistent with the configs.");

            foreach (var pair in pairs)
            {
                var keyName = GetKeyName(group, pair.Key.Name);
                _container.GroupsDic[group].KeyContentPairsDic[keyName].Content = pair.Content;
            }

            AutoSave();
        }

        /// <summary>
        /// Remove the specified key in default group.
        /// </summary>
        /// <param name="key">Key name or key alias name.</param>
        public void RemoveKey(string key)
        {
            RemoveKey(Identifier.DefaultGroupName, key);
        }

        /// <summary>
        /// Remove the specified key in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="key">Key name or key alias name.</param>
        public void RemoveKey(string group, string key)
        {
            if (!TryGetKeyName(group, key, out var keyName))
                ErrorHandler.This.Add(ErrorType.NotMatch);

            _container.GroupsDic[group].KeyContentPairsDic.Remove(keyName);
            _container.GroupsDic[group].OrderedLine.Remove(new TaggedLine(true, keyName));
            AutoSave();
        }

        /// <summary>
        /// Remove the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        public void RemoveGroup(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch);

            _container.GroupsDic.Remove(group);
            _container.OrderedLine.Remove(new TaggedLine(true, group));
            AutoSave();
        }

        /// <summary>
        /// Clear the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        public void ClearGroup(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch);

            _container.GroupsDic[group].KeyContentPairsDic.Clear();
            _container.GroupsDic[group].OrderedLine.Clear();
            AutoSave();
        }

        /// <summary>
        /// Save the config file if <see cref="BearML"/> is created with config file path.
        /// </summary>
        public void Save()
        {
            if (_writer != null) _writer.Write(_container);
        }

        /// <summary>
        /// Save a copy to the specified path.
        /// </summary>
        /// <param name="path">Path to save copy of the config file.</param>
        public void SaveCopyTo(string path)
        {
            new Writer(path).Write(_container);
        }

        /// <summary>
        /// Get raw text.
        /// </summary>
        /// <returns>Raw text.</returns>
        public string[] GetRawText()
        {
            return _container.Raw.ToArray();
        }

        private void AutoSave()
        {
            if (_autoSave) Save();
        }

        private string GetKeyName(string group, string key, bool aliasNameSensitive = true)
        {
            if(!TryGetKeyName(group, key, out var keyName, aliasNameSensitive))
                ErrorHandler.This.Add(ErrorType.NotMatch, $"{group}.{key}");

            return keyName;
        }

        private bool TryGetKeyName(string group, string key, out string keyName, bool aliasNameSensitive = true)
        {
            keyName = default;
            if (!_container.GroupsDic.ContainsKey(group)) return false;

            var keyContentPairsDic = _container.GroupsDic[group].KeyContentPairsDic;

            if (aliasNameSensitive)
            {
                foreach (var (_, pair) in keyContentPairsDic)
                {
                    if (pair.Key.Name == key || pair.Key.AliasName == key)
                    {
                        keyName = pair.Key.Name;
                        return true;
                    }
                }

                return false;
            }
            else
            {
                if (!keyContentPairsDic.ContainsKey(key)) return false;

                keyName = key;
                return true;
            }
        }

        private void ThrowError(ErrorArgs args)
        {
            switch (args.Type)
            {
                case ErrorType.TypeNotSupport:
                    throw new TypeNotSupportException(args.Message);
                case ErrorType.InvalidConfig:
                    throw new InvalidConfigException("Config specified is invalid. " + args.Message);
                case ErrorType.NotMatch:
                    throw new ConfigNotMatchException($"Cannot match {args.Message}.");
            }
        }
    }
}
