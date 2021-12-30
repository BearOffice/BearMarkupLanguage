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
    public class BearML
    {
        private readonly ConfigsContainer _container;
        private readonly Writer _writer;
        private readonly IConversionProvider[] _providers;
        private bool _autoSave;

        public BearML(string path, bool autoSave = true)
        {
            _container = new Reader(path).GenerateConfigs();
            _writer = new Writer(path);
            _providers = Array.Empty<IConversionProvider>();
            _autoSave = autoSave;

            ErrorHandler.This.Message += ThrowError;
        }

        public BearML(string path, IConversionProvider provider, bool autoSave = true)
        {
            _container = new Reader(path).GenerateConfigs();
            _writer = new Writer(path);
            _providers = new[] { provider };
            _autoSave = autoSave;

            ErrorHandler.This.Message += ThrowError;
        }

        public BearML(string path, IConversionProvider[] providers, bool autoSave = true)
        {
            _container = new Reader(path).GenerateConfigs();
            _writer = new Writer(path);
            _providers = providers;
            _autoSave = autoSave;

            ErrorHandler.This.Message += ThrowError;
        }

        public T GetContent<T>(string key)
        {
            return GetContent<T>(Identifier.DefaultGroupName, key);
        }

        public T GetContent<T>(string group, string key)
        {
            if (!TryGetContent<T>(group, key, out var content))
                ErrorHandler.This.Add(ErrorType.NotMatch, $"{group}.{key}");

            return content;
        }

        public bool TryGetContent<T>(string key, out T content)
        {
            return TryGetContent(Identifier.DefaultGroupName, key, out content);
        }

        public bool TryGetContent<T>(string group, string key, out T content)
        {
            content = default;
            if (!TryGetKeyName(group, key, out var keyName)) return false;
            
            content = _container.GroupsDic[group].KeyContentPairsDic[keyName].Content.Get<T>(_providers);
            return true;
        }

        public void ChangeContent<T>(string key, T content)
        {
            ChangeContent(Identifier.DefaultGroupName, key, content);
        }

        public void ChangeContent<T>(string group, string key, T content)
        {
            var keyName = GetKeyName(group, key);
            var contentObj = ContentTypeSelector.GetContent(content, _providers);

            _container.GroupsDic[group].KeyContentPairsDic[keyName].Content = contentObj;
            AutoSave();
        }

        public string GetComment(string key)
        {
            return GetComment(Identifier.DefaultGroupName, key);
        }

        public string GetComment(string group, string key)
        {
            if (!TryGetComment(group, key, out var comment))
                ErrorHandler.This.Add(ErrorType.NotMatch, $"{group}.{key}");
            
            return comment;
        }

        public bool TryGetComment(string key, out string comment)
        {
            return TryGetComment(Identifier.DefaultGroupName, key, out comment);
        }

        public bool TryGetComment(string group, string key, out string comment)
        {
            comment = default;
            if (!TryGetKeyName(group, key, out var keyName)) return false;

            comment = _container.GroupsDic[group].KeyContentPairsDic[keyName].Key.Comment;
            return true;
        }

        public void ChangeComment(string key, string comment)
        {
            ChangeComment(Identifier.DefaultGroupName, key, comment);
        }

        public void ChangeComment(string group, string key, string comment)
        {
            var keyName = GetKeyName(group, key);

            var currentKey = _container.GroupsDic[group].KeyContentPairsDic[keyName].Key;
            _container.GroupsDic[group].KeyContentPairsDic[keyName].Key =
                new Key(currentKey.Name, currentKey.AliasName, comment);

            AutoSave();
        }

        public string GetKeyAlias(string key)
        {
            return GetKeyAlias(Identifier.DefaultGroupName, key);
        }

        public string GetKeyAlias(string group, string key)
        {
            if (!TryGetKeyAlias(group, key, out var keyAlias))
                ErrorHandler.This.Add(ErrorType.NotMatch, $"{group}.{key}");

            return keyAlias;
        }

        public bool TryGetKeyAlias(string key, out string keyAlias)
        {
            return TryGetKeyAlias(Identifier.DefaultGroupName, key, out keyAlias);
        }

        public bool TryGetKeyAlias(string group, string key, out string keyAlias)
        {
            keyAlias = default;
            if (!TryGetKeyName(group, key, out var keyName)) return false;

            keyAlias = _container.GroupsDic[group].KeyContentPairsDic[keyName].Key.AliasName;
            return true;
        }

        public void ChangeKeyAlias(string key, string keyAlias)
        {
            ChangeKeyAlias(Identifier.DefaultGroupName, key, keyAlias);
        }

        public void ChangeKeyAlias(string group, string key, string keyAlias)
        {
            var keyName = GetKeyName(group, key);

            var currentKey = _container.GroupsDic[group].KeyContentPairsDic[keyName].Key;
            _container.GroupsDic[group].KeyContentPairsDic[keyName].Key =
                new Key(currentKey.Name, keyAlias, currentKey.Comment);

            AutoSave();
        }

        public string[] GetAllKeys(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch, group);

            return _container.GroupsDic[group].KeyContentPairsDic.Keys.ToArray();
        }

        public string[] GetAllGroups()
        {
            return _container.GroupsDic.Keys.ToArray();
        }

        public bool ContainsKey(string key)
        {
            return ContainsKey(Identifier.DefaultGroupName, key);
        }

        public bool ContainsKey(string group, string key)
        {
            return TryGetKeyName(group, key, out _);
        }

        public bool ContainsGroup(string group)
        {
            return _container.GroupsDic.ContainsKey(group);
        }

        public void AddKey<T>(string key, T content)
        {
            AddKey(Identifier.DefaultGroupName, key, null, null, content);
        }

        public void AddKey<T>(string group, string key, T content)
        {
            AddKey(group, key, null, null, content);
        }

        public void AddKey<T>(string key, string keyAlias, string comment, T content)
        {
            AddKey(Identifier.DefaultGroupName, key, keyAlias, comment, content);
        }

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

        public void AddEmptyGroup(string group)
        {
            if (ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.InvalidConfig, "Group already exists.");

            _container.GroupsDic.Add(group, new Group(group));
            _container.GroupsDic[group].OrderedLine.Add(new TaggedLine(false, string.Empty));  // at an empty line
            _container.GroupsDic[group].OrderedLine.Add(new TaggedLine(true, group));
            AutoSave();
        }

        public string GetGroupRawText()
        {
            return GetGroupRawText(Identifier.DefaultGroupName);
        }

        public string GetGroupRawText(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch, group);

            return GroupParser.ParseToRaw(_container.GroupsDic[group])
                              .Aggregate("", (acc, item) => acc + item + '\n')[..^1];
        }

        public T DeserializeObjectGroup<T>(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch, group);

            var pairs = _container.GroupsDic[group].KeyContentPairsDic.Values.ToArray();

            return ObjectSerializer.Deserialize<T>(pairs, _providers);
        }

        public void AddObjectGroup<T>(string group, T source)
        {
            if (ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.InvalidConfig, "Group already exists.");

            _container.GroupsDic.Add(group, new Group(group));
            _container.OrderedLine.Add(new TaggedLine(false, string.Empty));  // at an empty line
            _container.OrderedLine.Add(new TaggedLine(true, group));

            var pairs = ObjectSerializer.Serialize(source, _providers);

            foreach (var pair in pairs)
            {
                _container.GroupsDic[group].KeyContentPairsDic.Add(pair.Key.Name, pair);
                _container.GroupsDic[group].OrderedLine.Add(new TaggedLine(true, pair.Key.Name));
            }

            AutoSave();
        }

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

        public void RemoveKey(string key)
        {
            RemoveKey(Identifier.DefaultGroupName, key);
        }

        public void RemoveKey(string group, string key)
        {
            if (!TryGetKeyName(group, key, out var keyName))
                ErrorHandler.This.Add(ErrorType.NotMatch);

            _container.GroupsDic[group].KeyContentPairsDic.Remove(keyName);
            AutoSave();
        }

        public void RemoveGroup(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch);

            _container.GroupsDic.Remove(group);
            AutoSave();
        }

        public void ClearGroup(string group)
        {
            if (!ContainsGroup(group))
                ErrorHandler.This.Add(ErrorType.NotMatch);

            _container.GroupsDic[group].KeyContentPairsDic.Clear();
            AutoSave();
        }

        private void AutoSave()
        {
            if (_autoSave) Save();
        }

        public void Save()
        {
            _writer.Write(_container);
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
