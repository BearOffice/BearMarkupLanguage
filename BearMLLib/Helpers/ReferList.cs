using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Helpers
{
    internal class ReferList<T> : IEnumerable<T>
    {
        private readonly T[] _referArray;
        private readonly int _start;
        private readonly int _end;
        internal int Count => _end - _start + 1;

        internal ReferList(T[] array, int start, int end)
        {
            // start = 0, end = -1 means empty
            if (start < 0 || end > array.Length - 1 || start - end > 1)
                throw new IndexOutOfRangeException();

            _referArray = array;
            _start = start;
            _end = end;
        }

        internal ReferList(T[] array)
        {
            _referArray = array;
            _start = 0;
            _end = array.Length - 1;
        }

        internal T this[int index] => _referArray[_start + index];
        internal T this[Index index] 
        { 
            get
            {
                if (index.IsFromEnd)
                    return _referArray[_end - index.Value + 1];
                else
                    return _referArray[_start + index.Value];
            }
        }
        internal ReferList<T> this[Range range]
        {
            get
            {
                var start = range.Start.IsFromEnd ? 
                    _end - range.Start.Value : _start + range.Start.Value;

                var end = range.End.IsFromEnd ?
                    _end - range.End.Value + 1: _start + range.End.Value;

                return new ReferList<T>(_referArray, start, end - 1);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ReferListEnumerator<T>(_referArray, _start, _end);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class ReferListEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] _referArray;
        private readonly int _start;
        private readonly int _end;
        private int _currentPos;

        public T Current => _referArray[_currentPos];
        object IEnumerator.Current => Current;

        internal ReferListEnumerator(T[] array, int start, int end)
        {
            _referArray = array;
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
}
