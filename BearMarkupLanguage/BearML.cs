using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMarkupLanguage.Core;
using BearMarkupLanguage.Text;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Conversion;
using BearMarkupLanguage.Serialization;
using System.Security;
using BearMarkupLanguage.Interpretation;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Interpretation.Helpers;
using System.Diagnostics.Contracts;
using System.Xml.Linq;
using static System.Reflection.Metadata.BlobBuilder;
using BearMarkupLanguage.Elements.Helpers;

namespace BearMarkupLanguage;

/// <summary>
/// Class to read or modify BearML config file.
/// For more information: <a href="https://github.com/BearOffice/BearMarkupLanguageLib">github.com/BearOffice/</a>
/// </summary>
public class BearML
{
    private readonly RootBlock _rootBlock;
    private readonly Writer _writer;
    private readonly IConversionProvider[] _providers;
    /// <summary>
    /// Changes saving timing. 
    /// If <see langword="true"/>, changes will not be saved to BearML config file automatically.
    /// </summary>
    public bool DelayedSave { get; set; } = false;
    /// <summary>
    /// Format BearML config file automatically.
    /// </summary>
    public bool AutoFormat { get; set; } = false;

    /// <summary>
    /// Provides methods to read or modify BearML config file.
    /// </summary>
    /// <param name="path">Path of the config file.</param>
    public BearML(string path)
    {
        var result = new Reader(path).Read(out var lines);
        if (!result.IsSuccess) ThrowParseException(lines[result.Error.LineIndex], result.Error);

        _rootBlock = result.Value;
        _writer = new Writer(path);
        _providers = Array.Empty<IConversionProvider>();
    }

    /// <summary>
    /// Provides methods to read or modify BearML config file.
    /// </summary>
    /// <param name="path">Path of the config file</param>
    /// <param name="providers">Provides methods to convert the specified types.</param>
    public BearML(string path, IConversionProvider[] providers)
    {
        var result = new Reader(path).Read(out var lines);
        if (!result.IsSuccess) ThrowParseException(lines[result.Error.LineIndex], result.Error);

        _rootBlock = result.Value;
        _writer = new Writer(path);
        _providers = providers;
    }


    /// <summary>
    /// Get value from a key in root.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="key">Key or key alias.</param>
    /// <returns>Value with the specified type.</returns>
    public T GetValue<T>(string key)
    {
        var name = GetKeyName(key);

        return (T)_rootBlock.KeyValuesDic[new Key { Name = name }].ConvertTo(typeof(T), _providers);
    }

    /// <summary>
    /// Get value from a key in the specified block.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <returns>Value with the specified type.</returns>
    public T GetValue<T>(string block, string key)
    {
        return GetValue<T>(new[] { block }, key);
    }

    /// <summary>
    /// Get value from a key in the specified block.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <returns>Value with the specified type.</returns>
    public T GetValue<T>(string[] block, string key)
    {
        var nestedBlock = GetNestedBlock(block);
        var name = GetKeyName(nestedBlock, key);

        return (T)nestedBlock.KeyValuesDic[new Key { Name = name }].ConvertTo(typeof(T), _providers);
    }

    /// <summary>
    /// Try getting value from a key in root.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="key">Key or key alias.</param>
    /// <param name="value">Value with the specified type.</param>
    /// <returns><see langword="true"/> if succeed; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue<T>(string key, out T value)
    {
        value = default;

        if (!TryGetKeyName(key, out var name)) return false;

        value = (T)_rootBlock.KeyValuesDic[new Key { Name = name }].ConvertTo(typeof(T), _providers);
        return true;
    }

    /// <summary>
    /// Try getting value from a key in the specified block.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="value">Value with the specified type.</param>
    /// <returns><see langword="true"/> if succeed; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue<T>(string block, string key, out T value)
    {
        if (!TryGetValue(new[] { block }, key, out value)) return false;

        return true;
    }

    /// <summary>
    /// Try getting value from a key in the specified block.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="value">Value with the specified type.</param>
    /// <returns><see langword="true"/> if succeed; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue<T>(string[] block, string key, out T value)
    {
        value = default;

        if (!TryGetNestedBlock(block, out var nestedBlock)) return false;
        if (!TryGetKeyName(nestedBlock, key, out var name)) return false;

        value = (T)nestedBlock.KeyValuesDic[new Key { Name = name }].ConvertTo(typeof(T), _providers);
        return true;
    }

    /// <summary>
    /// Change value binded by a key in root.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="key">Key or key alias.</param>
    /// <param name="value">Value with the specified type.</param>
    public void ChangeValue<T>(string key, T value)
    {
        var keyName = new Key { Name = GetKeyName(key) };

        _rootBlock.KeyValuesDic[keyName] = ElementConverter.BuildElement(value, _providers);
        var taggedLine = _rootBlock.TaggedLines.Find(t => t.LineType == LineType.KeyValuePair && t.Key == keyName);
        taggedLine.ValueLines = null;

        AutoSave();
    }

    /// <summary>
    /// Change value binded by a key in the specified block.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="value">Value with the specified type.</param>
    public void ChangeValue<T>(string block, string key, T value)
    {
        ChangeValue(new[] { block }, key, value);
    }

    /// <summary>
    /// Change value binded by a key in the specified block.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="value">Value with the specified type.</param>
    public void ChangeValue<T>(string[] block, string key, T value)
    {
        var nestedBlock = GetNestedBlock(block);
        var keyName = new Key { Name = GetKeyName(nestedBlock, key) };

        nestedBlock.KeyValuesDic[keyName] = ElementConverter.BuildElement(value, _providers);
        var taggedLine = nestedBlock.TaggedLines.Find(t => t.LineType == LineType.KeyValuePair && t.Key == keyName);
        taggedLine.ValueLines = null;

        ResetTaggedValueLinesInSuperChain(block);
        AutoSave();
    }

    /// <summary>
    /// Get comment from a key in root.
    /// </summary>
    /// <param name="key">Key name or key alias.</param>
    /// <returns>Comment.</returns>
    public string GetComment(string key)
    {
        var name = GetKeyName(key);
        var comment = _rootBlock.KeyValuesDic.Keys.First(k => k.Name == name).Comment;

        return comment;
    }

    /// <summary>
    /// Get comment from a key in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <returns>Comment.</returns>
    public string GetComment(string block, string key)
    {
        return GetComment(new[] { block }, key);
    }

    /// <summary>
    /// Get comment from a key in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <returns>Comment.</returns>
    public string GetComment(string[] block, string key)
    {
        var nestedBlock = GetNestedBlock(block);
        var name = GetKeyName(nestedBlock, key);
        var comment = nestedBlock.KeyValuesDic.Keys.First(k => k.Name == name).Comment;

        return comment;
    }

    /// <summary>
    /// Change comment binded by a key in root.
    /// </summary>
    /// <param name="key">Key or key alias.</param>
    /// <param name="comment">Comment.</param>
    public void ChangeComment(string key, string comment)
    {
        var name = GetKeyName(key);
        var keyObj = _rootBlock.KeyValuesDic.Keys.First(k => k.Name == name);
        _rootBlock.KeyValuesDic.ChangeKey(keyObj, new Key
        {
            Name = keyObj.Name,
            Aliases = keyObj.Aliases,
            Comment = comment
        });

        var taggedLine = _rootBlock.TaggedLines.Find(t => t.LineType == LineType.KeyValuePair && t.Key == keyObj);
        taggedLine.KeyLines = null;

        AutoSave();
    }

    /// <summary>
    /// Change comment binded by a key in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="comment">Comment.</param>
    public void ChangeComment(string block, string key, string comment)
    {
        ChangeComment(new[] { block }, key, comment);
    }

    /// <summary>
    /// Change comment binded by a key in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="comment">Comment.</param>
    public void ChangeComment(string[] block, string key, string comment)
    {
        var nestedBlock = GetNestedBlock(block);
        var name = GetKeyName(nestedBlock, key);
        var keyObj = nestedBlock.KeyValuesDic.Keys.First(k => k.Name == name);
        nestedBlock.KeyValuesDic.ChangeKey(keyObj, new Key
        {
            Name = keyObj.Name,
            Aliases = keyObj.Aliases,
            Comment = comment
        });

        var taggedLine = nestedBlock.TaggedLines.Find(t => t.LineType == LineType.KeyValuePair && t.Key == keyObj);
        taggedLine.KeyLines = null;

        ResetTaggedValueLinesInSuperChain(block);
        AutoSave();
    }

    /// <summary>
    /// Remove comment binded by a key in root.
    /// </summary>
    /// <param name="key">Key or key alias.</param>
    public void RemoveComment(string key)
    {
        ChangeComment(key, null);
    }

    /// <summary>
    /// Remove comment binded by a key in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    public void RemoveComment(string block, string key)
    {
        RemoveComment(new[] { block }, key);
    }

    /// <summary>
    /// Remove comment binded by a key in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    public void RemoveComment(string[] block, string key)
    {
        ChangeComment(block, key, null);
    }

    /// <summary>
    /// Get key aliases from a key in root.
    /// </summary>
    /// <param name="key">Key or key alias.</param>
    /// <returns>Key aliases.</returns>
    public string[] GetKeyAliases(string key)
    {
        var name = GetKeyName(key);
        return _rootBlock.KeyValuesDic.Keys.First(k => k.Name == name).Aliases;
    }

    /// <summary>
    /// Get key aliases from a key in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <returns>Key aliases.</returns>
    public string[] GetKeyAliases(string block, string key)
    {
        return GetKeyAliases(new[] { block }, key);
    }

    /// <summary>
    /// Get key aliases from a key in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <returns>Key aliases.</returns>
    public string[] GetKeyAliases(string[] block, string key)
    {
        var nestedBlock = GetNestedBlock(block);
        var name = GetKeyName(nestedBlock, key);
        return nestedBlock.KeyValuesDic.Keys.First(k => k.Name == name).Aliases;
    }

    /// <summary>
    /// Change key aliases binded by a key in root.
    /// </summary>
    /// <param name="key">Key or key alias.</param>
    /// <param name="keyAliases">Array of key aliases. Key alias's format should be followed as below.
    /// <code>"^@?[a-zA-Z_][a-zA-Z_0-9]*$"</code></param>
    public void ChangeKeyAliases(string key, string[] keyAliases)
    {
        var name = GetKeyName(key);
        var keyObj = _rootBlock.KeyValuesDic.Keys.First(k => k.Name == name);

        _rootBlock.KeyValuesDic.ChangeKey(keyObj, new Key
        {
            Name = keyObj.Name,
            Aliases = ParseKeyAliases(keyAliases),
            Comment = keyObj.Comment
        });

        var taggedLine = _rootBlock.TaggedLines.Find(t => t.LineType == LineType.KeyValuePair && t.Key == keyObj);
        taggedLine.KeyLines = null;

        AutoSave();
    }

    /// <summary>
    /// Change key aliases binded by a key in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="keyAliases">Array of key aliases. Key alias's format should be followed as below.
    /// <code>"^@?[a-zA-Z_][a-zA-Z_0-9]*$"</code></param>
    public void ChangeKeyAliases(string block, string key, string[] keyAliases)
    {
        ChangeKeyAliases(new[] { block }, key, keyAliases);
    }

    /// <summary>
    /// Change key aliases binded by a key in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key name or key alias name.</param>
    /// <param name="keyAliases">Array of key aliases. Key alias's format should be followed as below.
    /// <code>"^@?[a-zA-Z_][a-zA-Z_0-9]*$"</code></param>
    public void ChangeKeyAliases(string[] block, string key, string[] keyAliases)
    {
        var nestedBlock = GetNestedBlock(block);
        var name = GetKeyName(nestedBlock, key);
        var keyObj = nestedBlock.KeyValuesDic.Keys.First(k => k.Name == name);
        nestedBlock.KeyValuesDic.ChangeKey(keyObj, new Key
        {
            Name = keyObj.Name,
            Aliases = ParseKeyAliases(keyAliases),
            Comment = keyObj.Comment
        });

        var taggedLine = nestedBlock.TaggedLines.Find(t => t.LineType == LineType.KeyValuePair && t.Key == keyObj);
        taggedLine.KeyLines = null;

        ResetTaggedValueLinesInSuperChain(block);
        AutoSave();
    }

    /// <summary>
    /// Remove key aliases binded by a key in root.
    /// </summary>
    /// <param name="key">Key or key alias.</param>
    public void RemoveKeyAliases(string key)
    {
        ChangeKeyAliases(key, null);
    }

    /// <summary>
    /// Remove key aliases binded by a key in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    public void RemoveKeyAliases(string block, string key)
    {
        RemoveKeyAliases(new[] { block }, key);
    }

    /// <summary>
    /// Remove key aliases binded by a key in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key name or key alias name.</param>
    public void RemoveKeyAliases(string[] block, string key)
    {
        ChangeKeyAliases(block, key, null);
    }

    private static string[] ParseKeyAliases(string[] aliases)
    {
        if (aliases is null) return null;

        var parsed = new string[aliases.Length];
        for (var i = 0; i < aliases.Length; i++)
        {
            parsed[i] = aliases[i].TrimStartAndEnd();
            if (!Key.IsValidAlias(parsed[i]))
                throw new InvalidFormatException("Invalid key alias format.");
        }

        return parsed;
    }

    /// <summary>
    /// Change key name in root.
    /// </summary>
    /// <param name="key">Key or key alias.</param>
    /// <param name="newName">New key name.</param>
    public void ChangeKeyName(string key, string newName)
    {
        var name = GetKeyName(key);
        var keyObj = _rootBlock.KeyValuesDic.Keys.First(k => k.Name == name);
        var newKey = new Key
        {
            Name = ParseKeyName(newName),
            Aliases = keyObj.Aliases,
            Comment = keyObj.Comment
        };

        if (newKey != keyObj && _rootBlock.KeyValuesDic.ContainsKey(newKey))
            throw new ArgumentException("Key must be unique.");

        _rootBlock.KeyValuesDic.ChangeKey(keyObj, newKey);

        var taggedLine = _rootBlock.TaggedLines.Find(t => t.LineType == LineType.KeyValuePair && t.Key == keyObj);
        taggedLine.KeyLines = null;

        AutoSave();
    }

    /// <summary>
    /// Change key name in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="newName">New key name.</param>
    public void ChangeKeyName(string block, string key, string newName)
    {
        ChangeKeyName(new[] { block }, key, newName);
    }

    /// <summary>
    /// Change key name in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <param name="newName">New key name.</param>
    public void ChangeKeyName(string[] block, string key, string newName)
    {
        var nestedBlock = GetNestedBlock(block);
        var name = GetKeyName(nestedBlock, key);
        var keyObj = nestedBlock.KeyValuesDic.Keys.First(k => k.Name == name);
        var newKey = new Key
        {
            Name = ParseKeyName(newName),
            Aliases = keyObj.Aliases,
            Comment = keyObj.Comment
        };

        if (newKey != keyObj && nestedBlock.KeyValuesDic.ContainsKey(newKey))
            throw new ArgumentException("Key must be unique.");

        nestedBlock.KeyValuesDic.ChangeKey(keyObj, newKey);

        var taggedLine = nestedBlock.TaggedLines.Find(t => t.LineType == LineType.KeyValuePair && t.Key == keyObj);
        taggedLine.KeyLines = null;

        ResetTaggedValueLinesInSuperChain(block);
        AutoSave();
    }

    /// <summary>
    /// Get comment from the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <returns>Comment.</returns>
    public string GetBlockComment(string block)
    {
        return GetBlockComment(new[] { block });
    }

    /// <summary>
    /// Get comment from the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <returns>Comment.</returns>
    public string GetBlockComment(string[] block)
    {
        _ = GetNestedBlock(block);  // check if block exists.

        if (block.Length == 1)
        {
            var keyObj = _rootBlock.BlocksDic.Keys.First(k => k.Name == block[0]);
            return keyObj.Comment;
        }
        else
        {
            var superBlock = GetNestedBlock(block[..^1]);
            var keyObj = superBlock.BlocksDic.Keys.First(k => k.Name == block[^1]);
            return keyObj.Comment;
        }
    }

    /// <summary>
    /// Change comment binded by the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="comment">Comment.</param>
    /// <returns>Comment.</returns>
    public void ChangeBlockComment(string block, string comment)
    {
        ChangeBlockComment(new[] { block }, comment);
    }

    /// <summary>
    /// Change comment binded by the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="comment">Comment.</param>
    /// <returns>Comment.</returns>
    public void ChangeBlockComment(string[] block, string comment)
    {
        _ = GetNestedBlock(block);  // check if block exists.

        if (block.Length == 1)
        {
            var keyObj = _rootBlock.BlocksDic.Keys.First(k => k.Name == block[0]);
            _rootBlock.BlocksDic.ChangeKey(keyObj, new BlockKey
            {
                Name = keyObj.Name,
                Comment = comment
            });
        }
        else
        {
            var superBlock = GetNestedBlock(block[..^1]);
            var keyObj = superBlock.BlocksDic.Keys.First(k => k.Name == block[^1]);
            superBlock.BlocksDic.ChangeKey(keyObj, new BlockKey
            {
                Name = keyObj.Name,
                Comment = comment
            });
        }

        ResetTaggedKeyLinesInSuperChain(block);
        AutoSave();
    }

    /// <summary>
    /// Remove comment binded by the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <returns>Comment.</returns>
    public void RemoveBlockComment(string block)
    {
        RemoveBlockComment(new[] { block });
    }

    /// <summary>
    /// Remove comment binded by the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <returns>Comment.</returns>
    public void RemoveBlockComment(string[] block)
    {
        ChangeBlockComment(block, null);
    }

    /// <summary>
    /// Change name of the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="newName">New key name.</param>
    public void ChangeBlockName(string block, string newName)
    {
        ChangeBlockName(new[] { block }, newName);
    }

    /// <summary>
    /// Change name of the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="newName">New key name.</param>
    public void ChangeBlockName(string[] block, string newName)
    {
        _ = GetNestedBlock(block);

        if (block.Length == 1)
        {
            var keyObj = _rootBlock.BlocksDic.Keys.First(k => k.Name == block[0]);
            var newKey = new BlockKey
            {
                Name = ParseBlockName(newName),
                Comment = keyObj.Comment
            };
            if (newKey != keyObj && _rootBlock.BlocksDic.ContainsKey(newKey))
                throw new ArgumentException("Key must be unique.");

            _rootBlock.BlocksDic.ChangeKey(keyObj, newKey);
        }
        else
        {
            var superBlock = GetNestedBlock(block[..^1]);
            var keyObj = superBlock.BlocksDic.Keys.First(k => k.Name == block[0]);
            var newKey = new BlockKey
            {
                Name = ParseBlockName(newName),
                Comment = keyObj.Comment
            };
            if (newKey != keyObj && superBlock.BlocksDic.ContainsKey(newKey))
                throw new ArgumentException("Key must be unique.");

            superBlock.BlocksDic.ChangeKey(keyObj, newKey);
        }

        ResetTaggedKeyLinesInSuperChain(block);
        AutoSave();
    }

    /// <summary>
    /// Get all keys in root. Keys are not shadowed by aliases.
    /// </summary>
    public string[] GetAllKeys()
    {
        return _rootBlock.KeyValuesDic.Keys.Select(k => k.Name).ToArray();
    }

    /// <summary>
    /// Get all keys in the specified block. Keys are not shadowed by aliases.
    /// </summary>
    /// <param name="block">Block name.</param>
    public string[] GetAllKeys(string block)
    {
        return GetAllKeys(new[] { block });
    }

    /// <summary>
    /// Get all keys in the specified block. Keys are not shadowed by aliases.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    public string[] GetAllKeys(string[] block)
    {
        var nestedBlock = GetNestedBlock(block);
        return nestedBlock.KeyValuesDic.Keys.Select(k => k.Name).ToArray();
    }

    /// <summary>
    /// Get all sub block names in root.
    /// </summary>
    public string[] GetAllSubBlockNames()
    {
        return _rootBlock.BlocksDic.Keys.Select(k => k.Name).ToArray();
    }

    /// <summary>
    /// Get all sub block names in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    public string[] GetAllSubBlockNames(string block)
    {
        return GetAllSubBlockNames(new[] { block });
    }

    /// <summary>
    /// Get all sub block names in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    public string[] GetAllSubBlockNames(string[] block)
    {
        var nestedBlock = GetNestedBlock(block);
        return nestedBlock.BlocksDic.Keys.Select(k => k.Name).ToArray();
    }

    /// <summary>
    /// Determines whether root contains the specified key.
    /// </summary>
    /// <param name="key">Key or key alias.</param>
    /// <returns><see langword="true"/> if key exists; otherwise, <see langword="false"/></returns>
    public bool ContainsKey(string key)
    {
        return TryGetKeyName(key, out _);
    }

    /// <summary>
    /// Determines whether the specified block contains the specified key.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <returns><see langword="true"/> if key exists; otherwise, <see langword="false"/></returns>
    public bool ContainsKey(string block, string key)
    {
        return ContainsKey(new[] { block }, key);
    }

    /// <summary>
    /// Determines whether the specified block contains the specified key.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    /// <returns><see langword="true"/> if key exists; otherwise, <see langword="false"/></returns>
    public bool ContainsKey(string[] block, string key)
    {
        if (!TryGetNestedBlock(block, out var nestedBlock))
            return false;

        return TryGetKeyName(nestedBlock, key, out _);
    }

    /// <summary>
    /// Determines whether root contains the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <returns><see langword="true"/> if block exists; otherwise, <see langword="false"/></returns>
    public bool ContainsBlock(string block)
    {
        return ContainsBlock(new[] { block });
    }

    /// <summary>
    /// Determines whether root contains the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <returns><see langword="true"/> if block exists; otherwise, <see langword="false"/></returns>
    public bool ContainsBlock(string[] block)
    {
        return TryGetNestedBlock(block, out _);
    }

    /// <summary>
    /// Add key and value to root.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="key">Key name.</param>
    /// <param name="value">Value with the specified type.</param>
    public void AddKeyValue<T>(string key, T value)
    {
        var keyObj = new Key { Name = ParseKeyName(key) };
        if (_rootBlock.KeyValuesDic.ContainsKey(keyObj))
            throw new ArgumentException("Key must be unique.");

        _rootBlock.KeyValuesDic.Add(keyObj, ElementConverter.BuildElement(value, _providers));
        _rootBlock.TaggedLines.Add(new TaggedLine { LineType = LineType.KeyValuePair, Key = keyObj });
        _rootBlock.SetClearParseStatus();
        AutoSave();
    }

    /// <summary>
    /// Add key and value to the specified block.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key name.</param>
    /// <param name="value">Value with the specified type.</param>
    public void AddKeyValue<T>(string block, string key, T value)
    {
        AddKeyValue(new[] { block }, key, value);
    }

    /// <summary>
    /// Add key and value to the specified block.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key name.</param>
    /// <param name="value">Value with the specified type.</param>
    public void AddKeyValue<T>(string[] block, string key, T value)
    {
        var keyObj = new Key { Name = ParseKeyName(key) };
        var nestedBlock = GetNestedBlock(block);
        if (nestedBlock.KeyValuesDic.ContainsKey(keyObj))
            throw new ArgumentException("Key must be unique.");

        nestedBlock.KeyValuesDic.Add(keyObj, ElementConverter.BuildElement(value, _providers));
        nestedBlock.TaggedLines.Add(new TaggedLine { LineType = LineType.KeyValuePair, Key = keyObj });
        nestedBlock.SetClearParseStatus();
        ResetTaggedValueLinesInSuperChain(block);
        AutoSave();
    }

    /// <summary>
    /// Add an empty block to root.
    /// </summary>
    /// <param name="block">Block name.</param>
    public void AddEmptyBlock(string block)
    {
        var keyObj = new BlockKey { Name = ParseBlockName(block) };
        if (_rootBlock.BlocksDic.ContainsKey(keyObj))
            throw new ArgumentException("Block name must be unique.");

        _rootBlock.BlocksDic.Add(keyObj, new Block
        {
            BlocksDic = new OrderedDictionary<BlockKey, Block>(),
            KeyValuesDic = new OrderedDictionary<Key, IBaseElement>(),
            TaggedLines = new List<TaggedLine>()
        });
        // _rootBlock.TaggedLines.Add(new TaggedLine { LineType = LineType.Block, BlockKey = keyObj });
        _rootBlock.SetClearParseStatus();  // If this flag is set, tagged lines are ignored.
        AutoSave();
    }

    /// <summary>
    /// Add an empty block to the specified block.
    /// </summary>
    /// <param name="superBlock">Hierarchy of super block name.</param>
    /// <param name="block">Block name.</param>
    public void AddEmptyBlock(string[] superBlock, string block)
    {
        var keyObj = new BlockKey { Name = ParseBlockName(block) };

        var super = GetNestedBlock(superBlock);
        if (super.BlocksDic.ContainsKey(keyObj))
            throw new ArgumentException("Block name must be unique.");

        super.BlocksDic.Add(keyObj, new Block
        {
            BlocksDic = new OrderedDictionary<BlockKey, Block>(),
            KeyValuesDic = new OrderedDictionary<Key, IBaseElement>(),
            TaggedLines = new List<TaggedLine>()
        });
        // super.TaggedLines.Add(new TaggedLine { LineType = LineType.Block, BlockKey = keyObj });
        super.SetClearParseStatus();
        ResetTaggedValueLinesInSuperChain(superBlock);
        AutoSave();
    }

    /// <summary>
    /// Remove the specified key in root.
    /// </summary>
    /// <param name="key">Key or key alias.</param>
    public void RemoveKey(string key)
    {
        var name = GetKeyName(key);
        var keyObj = new Key { Name = name };

        _rootBlock.KeyValuesDic.Remove(keyObj);
        var tagIndex = _rootBlock.TaggedLines.FindIndex(t => t.Key == keyObj);
        _rootBlock.TaggedLines.RemoveAt(tagIndex);

        AutoSave();
    }

    /// <summary>
    /// Remove the specified key in the specified block.
    /// </summary>
    /// <param name="block">Block name.</param>
    /// <param name="key">Key or key alias.</param>
    public void RemoveKey(string block, string key)
    {
        RemoveKey(new[] { block }, key);
    }

    /// <summary>
    /// Remove the specified key in the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    /// <param name="key">Key or key alias.</param>
    public void RemoveKey(string[] block, string key)
    {
        var nestedBlock = GetNestedBlock(block);
        var name = GetKeyName(nestedBlock, key);
        var keyObj = new Key { Name = name };

        nestedBlock.KeyValuesDic.Remove(keyObj);
        var tagIndex = nestedBlock.TaggedLines.FindIndex(t => t.LineType == LineType.KeyValuePair && t.Key == keyObj);
        nestedBlock.TaggedLines.RemoveAt(tagIndex);
        ResetTaggedValueLinesInSuperChain(block);
        AutoSave();
    }

    /// <summary>
    /// Remove the specified block in root.
    /// </summary>
    /// <param name="block">Block name.</param>
    public void RemoveBlock(string block)
    {
        RemoveBlock(new[] { block });
    }

    /// <summary>
    /// Remove the specified block in root.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    public void RemoveBlock(string[] block)
    {
        _ = GetNestedBlock(block);  // check if block exists

        if (block.Length == 1)
        {
            var blockKey = new BlockKey { Name = block[0] };
            _rootBlock.BlocksDic.Remove(blockKey);

            var tagIndex = _rootBlock.TaggedLines.FindIndex(
                t => t.LineType == LineType.Block && t.BlockKey == blockKey);
            _rootBlock.TaggedLines.RemoveAt(tagIndex);
        }
        else
        {
            var blockKey = new BlockKey { Name = block[^1] };
            var super = GetNestedBlock(block[..^1]);
            super.BlocksDic.Remove(blockKey);

            var tagIndex = super.TaggedLines.FindIndex(
                t => t.LineType == LineType.Block && t.BlockKey == blockKey);
            super.TaggedLines.RemoveAt(tagIndex);

            ResetTaggedValueLinesInSuperChain(block[..^1]);
        }

        AutoSave();
    }

    /// <summary>
    /// Clear the specified block in root.
    /// </summary>
    /// <param name="block">Block name.</param>
    public void ClearBlock(string block)
    {
        ClearBlock(new[] { block });
    }

    /// <summary>
    /// Clear the specified block.
    /// </summary>
    /// <param name="block">Hierarchy of block name.</param>
    public void ClearBlock(string[] block)
    {
        var nestedBlock = GetNestedBlock(block);
        nestedBlock.KeyValuesDic.Clear();
        nestedBlock.BlocksDic.Clear();
        nestedBlock.TaggedLines.Clear();
        nestedBlock.SetClearParseStatus();

        ResetTaggedValueLinesInSuperChain(block);
        AutoSave();
    }

    private void ResetTaggedValueLinesInSuperChain(string[] block)
    {
        var blockKey = new BlockKey { Name = block[0] };
        var tag = _rootBlock.TaggedLines.Find(
            t => t.LineType == LineType.Block && t.BlockKey == blockKey);
        tag.ValueLines = null;

        var currentBlock = _rootBlock.BlocksDic[blockKey];
        for (var i = 1; i < block.Length; i++)
        {
            blockKey = new BlockKey { Name = block[i] };
            tag = currentBlock.TaggedLines.Find(
                t => t.LineType == LineType.Block && t.BlockKey == blockKey);
            tag.ValueLines = null;

            currentBlock = currentBlock.BlocksDic[blockKey];
        }
    }

    private void ResetTaggedKeyLinesInSuperChain(string[] block)
    {
        var blockKey = new BlockKey { Name = block[0] };
        var tag = _rootBlock.TaggedLines.Find(
            t => t.LineType == LineType.Block && t.BlockKey == blockKey);
        tag.KeyLines = null;

        var currentBlock = _rootBlock.BlocksDic[blockKey];
        for (var i = 1; i < block.Length; i++)
        {
            blockKey = new BlockKey { Name = block[i] };
            tag = currentBlock.TaggedLines.Find(
                t => t.LineType == LineType.Block && t.BlockKey == blockKey);
            tag.KeyLines = null;

            currentBlock = currentBlock.BlocksDic[blockKey];
        }
    }

    private static string ParseKeyName(string key)
    {
        if (!Key.IsValidName(key))
            throw new InvalidFormatException("Key cannot be empty.");

        return key.TrimStartAndEnd();
    }

    private static string ParseBlockName(string key)
    {
        if (!BlockKey.IsValidName(key))
            throw new InvalidFormatException("Block name cannot be empty.");

        return key.TrimStartAndEnd();
    }

    private bool TryGetKeyName(string keyOrAlias, out string key)
    {
        key = default;
        var first = _rootBlock.KeyValuesDic.FirstOrDefault(pair => pair.Key.Name == keyOrAlias
            || (pair.Key.Aliases is not null && pair.Key.Aliases.Contains(keyOrAlias)));

        if (first.Key.Name is null)
        {
            return false;
        }
        else
        {
            key = first.Key.Name;
            return true;
        }
    }

    private static bool TryGetKeyName(Block block, string keyOrAlias, out string key)
    {
        key = default;

        var first = block.KeyValuesDic.FirstOrDefault(pair => pair.Key.Name == keyOrAlias
            || (pair.Key.Aliases is not null && pair.Key.Aliases.Contains(keyOrAlias)));

        if (first.Key.Name is null)
        {
            return false;
        }
        else
        {
            key = first.Key.Name;
            return true;
        }
    }

    private string GetKeyName(string keyOrAlias)
    {
        var first = _rootBlock.KeyValuesDic.FirstOrDefault(pair => pair.Key.Name == keyOrAlias
            || (pair.Key.Aliases is not null && pair.Key.Aliases.Contains(keyOrAlias)));

        if (first.Key.Name is null)
        {
            throw new KeyNotFoundException($"Cannot found key or key alias '{keyOrAlias}'.");
        }
        else
        {
            return first.Key.Name;
        }
    }

    private static string GetKeyName(Block block, string keyOrAlias)
    {
        var first = block.KeyValuesDic.FirstOrDefault(pair => pair.Key.Name == keyOrAlias
            || (pair.Key.Aliases is not null && pair.Key.Aliases.Contains(keyOrAlias)));

        if (first.Key.Name is null)
        {
            throw new KeyNotFoundException($"Cannot found key or key alias '{keyOrAlias}'.");
        }
        else
        {
            return first.Key.Name;
        }
    }

    private Block GetNestedBlock(string[] block)
    {
        if (block is null || block.Length == 0)
            throw new ArgumentException("Block name specified cannot be empty.");

        var nestedBlockKey = new BlockKey { Name = block[0] };
        if (!_rootBlock.BlocksDic.ContainsKey(nestedBlockKey))
            throw new KeyNotFoundException($"Cannot found block '{block[0]}'.");

        var nestedBlock = _rootBlock.BlocksDic[nestedBlockKey];
        foreach (var blockKey in block.Skip(1))
        {
            nestedBlockKey = new BlockKey { Name = blockKey };
            if (!nestedBlock.BlocksDic.ContainsKey(nestedBlockKey))
                throw new KeyNotFoundException($"Cannot found block '{blockKey}'.");

            nestedBlock = nestedBlock.BlocksDic[nestedBlockKey];
        }

        return nestedBlock;
    }

    private bool TryGetNestedBlock(string[] block, out Block nestedBlock)
    {
        nestedBlock = default;

        if (block is null || block.Length == 0)
            throw new ArgumentException("Block name specified cannot be empty.");

        var nestedBlockKey = new BlockKey { Name = block[0] };
        if (!_rootBlock.BlocksDic.ContainsKey(nestedBlockKey)) return false;

        nestedBlock = _rootBlock.BlocksDic[nestedBlockKey];
        foreach (var blockKey in block.Skip(1))
        {
            nestedBlockKey = new BlockKey { Name = blockKey };
            if (!nestedBlock.BlocksDic.ContainsKey(nestedBlockKey)) return false;

            nestedBlock = nestedBlock.BlocksDic[nestedBlockKey];
        }

        return true;
    }

    /// <summary>
    /// Format BearML config file.
    /// </summary>
    public void FormatFile()
    {
        _rootBlock.SetClearParseStatus();
        AutoSave();
    }

    /// <summary>
    /// Save the BearML config file.
    /// </summary>
    public void Save()
    {
        _writer.Write(_rootBlock, AutoFormat);
    }

    /// <summary>
    /// Save a BearML config file's copy to the specified path.
    /// </summary>
    /// <param name="path">Path to save copy of the config file.</param>
    public void SaveTo(string path)
    {
        new Writer(path).Write(_rootBlock, AutoFormat);
    }

    private void AutoSave()
    {
        if (!DelayedSave) Save();
    }

    /// <summary>
    /// Get root's literals.
    /// </summary>
    /// <returns>Raw text.</returns>
    public string[] GetLiterals()
    {
        return _rootBlock.ParseToLiteral();
    }

    /// <summary>
    /// Visualize an object by bear markup language.
    /// </summary>
    /// <typeparam name="T">The type of source object.</typeparam>
    /// <param name="source">Source object.</param>
    /// <param name="providers">Conversion providers.</param>
    /// <returns>Visualized result.</returns>
    public static string VisualizeObject<T>(T source, IConversionProvider[] providers = null)
    {
        providers ??= Array.Empty<IConversionProvider>();

        var element = ElementConverter.BuildElement(source, providers);
        var literals = element.ParseToLiteral(element.PreferredParseMode);

        var sb = new StringBuilder();
        foreach (var literal in literals.SkipLast(1))
        {
            sb.Append(literal).Append('\n');
        }

        if (literals.Length > 0)
            sb.Append(literals[^1]);

        return sb.ToString();
    }

    /// <summary>
    /// Serialize an object by bear markup language.
    /// </summary>
    /// <typeparam name="T">The type of source object.</typeparam>
    /// <param name="source">Source object.</param>
    /// <param name="providers">Conversion providers.</param>
    /// <returns>Serialized result.</returns>
    public static string Serialize<T>(T source, IConversionProvider[] providers = null)
    {
        providers ??= Array.Empty<IConversionProvider>();

        var element = ElementConverter.BuildElement(source, providers);
        var literals = RootBlock.ValueParse(element);

        var sb = new StringBuilder();
        foreach (var literal in literals.SkipLast(1))
        {
            sb.Append(literal).Append('\n');
        }

        if (literals.Length > 0)
            sb.Append(literals[^1]);

        return sb.ToString();
    }

    /// <summary>
    /// Deserialize literal in bear markup language to an object.
    /// </summary>
    /// <typeparam name="T">The type of target object.</typeparam>
    /// <param name="literal">The literal in bear markup language.</param>
    /// <param name="providers">Conversion providers.</param>
    /// <returns>Deserialized result.</returns>
    public static T Deserialize<T>(string literal, IConversionProvider[] providers = null)
    {
        providers ??= Array.Empty<IConversionProvider>();

        var literals = new ReferList<string>(literal.SplitToLines());
        var result = ContextInterpreter.ContentInterprete(literals, out _);

        if (!result.IsSuccess)
            ThrowParseException(literals[result.Error.LineIndex], result.Error);

        return (T)result.Value.ConvertTo(typeof(T), providers);
    }

    private static void ThrowParseException(string errLine, InvalidFormatExceptionArgs args)
    {
        var errMsg = ExceptionVisualizer.BuildMessage(errLine, args.LineIndex, args.CharIndex, args.Message);

        throw new InvalidFormatException(errMsg);
    }
}
