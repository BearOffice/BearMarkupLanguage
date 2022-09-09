using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BearMarkupLanguage.Helpers;

internal class ReferList<T> : IEnumerable<T>
{
    private readonly T[] _referArray;
    private readonly T[] _diffArray;
    private readonly int _start;
    private readonly int _end;
    internal int Count => _end - _start + 1;

    internal ReferList(T[] refArray, int start, int end)
    {
        // start = 0, end = -1 means empty
        if (start < 0 || end > refArray.Length - 1 || start - end > 1)
            throw new IndexOutOfRangeException();

        _referArray = refArray;
        _diffArray = new T[_referArray.Length];
        _start = start;
        _end = end;
    }

    internal ReferList(T[] array)
    {
        _referArray = array;
        _diffArray = new T[_referArray.Length];
        _start = 0;
        _end = array.Length - 1;
    }

    private ReferList(T[] refArray, T[] diffArray, int start, int end)
    {
        // start = 0, end = -1 means empty
        if (start < 0 || end > refArray.Length - 1 || start - end > 1)
            throw new IndexOutOfRangeException();

        _referArray = refArray;
        _diffArray = diffArray;
        _start = start;
        _end = end;
    }

    internal T this[int index]
    {
        get
        {
            if (_diffArray[_start + index] is null)
                return _referArray[_start + index];
            else
                return _diffArray[_start + index];
        }
        set
        {
            _diffArray[_start + index] = value;
        }
    }
    internal T this[Index index]
    {
        get
        {
            if (index.IsFromEnd)
                return _referArray[_end - index.Value + 1];
            else
                return _referArray[_start + index.Value];
        }
        set
        {
            if (index.IsFromEnd)
                _diffArray[_end - index.Value + 1] = value;
            else
                _diffArray[_start + index.Value] = value;
        }
    }
    internal ReferList<T> this[Range range]
    {
        get
        {
            var start = range.Start.IsFromEnd ?
                _end - range.Start.Value : _start + range.Start.Value;

            var end = range.End.IsFromEnd ?
                _end - range.End.Value + 1 : _start + range.End.Value;

            return new ReferList<T>(_referArray, (T[])_diffArray.Clone(), start, end - 1);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new ReferListEnumerator<T>(_referArray, _diffArray, _start, _end);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class ReferListEnumerator<T> : IEnumerator<T>
{
    private readonly T[] _referArray;
    private readonly T[] _diffArray;
    private readonly int _start;
    private readonly int _end;
    private int _currentPos;
    public T Current
    {
        get
        {
            if (_diffArray[_currentPos] is null)
                return _referArray[_currentPos];
            else
                return _diffArray[_currentPos];
        }
    }
    object IEnumerator.Current => Current;

    internal ReferListEnumerator(T[] referArray, T[] diffArray, int start, int end)
    {
        _referArray = referArray;
        _diffArray = diffArray;
        _start = start;
        _end = end;
        _currentPos = start - 1;
    }

    public bool MoveNext()
    {
        if (_currentPos < _end)
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
        _currentPos = _start - 1;
    }

    public void Dispose() { }
}
