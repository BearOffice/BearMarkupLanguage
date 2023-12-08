using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BearMarkupLanguage.Helpers;

internal class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly List<TKey> _orderedKeys;

    internal OrderedDictionary()
    {
        _dictionary = new Dictionary<TKey, TValue>();
        _orderedKeys = new List<TKey>();
    }

    internal OrderedDictionary(KeyValuePair<TKey, TValue>[] keyValuePairs)
    {
        _dictionary = new Dictionary<TKey, TValue>();
        _orderedKeys = new List<TKey>();

        foreach ((var key, var value) in keyValuePairs)
        {
            _dictionary.Add(key, value);
            _orderedKeys.Add(key);
        }
    }

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    public ICollection<TKey> Keys => _orderedKeys;
    public ICollection<TValue> Values => _orderedKeys.Select(key => _dictionary[key]).ToList();
    public int Count => _dictionary.Count;
    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        _orderedKeys.Add(key);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        _dictionary.Add(item.Key, item.Value);
        _orderedKeys.Add(item.Key);
    }

    public bool ChangeKey(TKey oldKey, TKey newKey)
    {
        if (!_dictionary.ContainsKey(oldKey)) return false;
        if (!oldKey.Equals(newKey) && _dictionary.ContainsKey(newKey)) return false;

        var value = _dictionary[oldKey];
        _dictionary.Remove(oldKey);
        _dictionary.Add(newKey, value);

        var index = _orderedKeys.IndexOf(oldKey);
        _orderedKeys[index] = newKey;

        return true;
    }

    public void Clear()
    {
        _dictionary.Clear();
        _orderedKeys.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _dictionary.Contains(item);
    }

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        _orderedKeys.Select(key => new KeyValuePair<TKey, TValue>(key, _dictionary[key]))
                     .ToList()
                     .CopyTo(array, arrayIndex);
    }

    public bool Remove(TKey key)
    {
        var result = _dictionary.Remove(key);
        _orderedKeys.Remove(key);

        return result;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var result = _dictionary.Remove(item.Key);
        _orderedKeys.Remove(item.Key);

        return result;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new OrderedDictionaryEnumerator<TKey, TValue>(_dictionary, _orderedKeys);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class OrderedDictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly List<TKey> _orderKeyList;
    private int _currentPos;

    public KeyValuePair<TKey, TValue> Current =>
        new KeyValuePair<TKey, TValue>(_orderKeyList[_currentPos], _dictionary[_orderKeyList[_currentPos]]);

    object IEnumerator.Current => Current;

    internal OrderedDictionaryEnumerator(Dictionary<TKey, TValue> dictionary, List<TKey> orderKeyList)
    {
        _dictionary = dictionary;
        _orderKeyList = orderKeyList;
        _currentPos = -1;
    }

    public bool MoveNext()
    {
        if (_currentPos < _orderKeyList.Count - 1)
        {
            _currentPos++;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Reset()
    {
        _currentPos = -1;
    }

    public void Dispose() { }
}
