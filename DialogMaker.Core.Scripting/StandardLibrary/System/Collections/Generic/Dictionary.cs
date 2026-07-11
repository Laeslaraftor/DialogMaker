namespace System.Collections.Generic;

public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : notnull
{
    public Dictionary() : this(4)
    {
    }
    public Dictionary(int capacity)
    {
        capacity = Math.Max(capacity, 4);
        _pairs = new KeyValuePair<TKey, TValue>[capacity];
    }

    public int Capacity => _pairs.Length;
    public int Count { get; private set; }
    public TValue this[TKey key]
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                var pair = _pairs[i];

                if (pair.Key == key)
                {
                    return pair.Value;
                }
            }

            throw new KeyNotFoundException();
        }
        set
        {
            for (int i = 0; i < Count; i++)
            {
                var pair = _pairs[i];

                if (pair.Key == key)
                {
                    _pairs[i] = new(pair.Key, value);
                    return;
                }
            }

            throw new KeyNotFoundException();
        }
    }

    private KeyValuePair<TKey, TValue>[] _pairs;

    public bool ContainsKey(TKey key)
    {
        for (int i = 0; i < Count; i++)
        {
            if (_pairs[i].Key == key)
            {
                return true;
            }
        }

        return false;
    }
    public void Add(TKey key, TValue value)
    {
        if (!TryAdd(key, value))
        {
            throw new ArgumentException("Value with same key already added");
        }
    }
    public bool TryAdd(TKey key, TValue value)
    {
        if (ContainsKey(key))
        {
            return false;
        }

        ExpandIfNeed();
        _pairs[Count] = new(key, value);
        Count++;

        return true;
    }
    public bool Remove(TKey key)
    {
        bool isFound = false;

        for (int i = 0; i < Count; i++)
        {
            if (_pairs[i].Key == key)
            {
                isFound = true;
                continue;
            }
            if (!isFound)
            {
                continue;
            }

            _pairs[i - 1] = _pairs[i];
        }

        if (isFound)
        {
            Count--;
        }

        return isFound;
    }
    public void Clear()
    {
        for (int i = 0; i < Count; i++)
        {
            _pairs[i] = null;
        }

        Count = 0;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new Enumerator(_pairs, Count);
    }

    private void ExpandIfNeed()
    {
        if (Count + 1 > Capacity)
        {
            Expand();
        }
    }
    private void Expand()
    {
        var newItems = new KeyValuePair<TKey, TValue>[_pairs.Length * 2];
        Array<KeyValuePair<TKey, TValue>>.Copy(_pairs, newItems);
        _pairs = newItems;
    }  

    private class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        public Enumerator(KeyValuePair<TKey, TValue>[] items, int count)
        {
            _items = items;
            _count = count;
        }

        public KeyValuePair<TKey, TValue> Current { get; private set; }

        private readonly KeyValuePair<TKey, TValue>[] _items;
        private readonly int _count;        
        private int _currentIndex = -1;

        public bool MoveNext()
        {
            if (_count == 0 || _currentIndex + 1 >= _count)
            {
                return false;
            }

            _currentIndex++;
            Current = _items[_currentIndex];

            return true;
        }
        public void Reset()
        {
            Current = null;
            _currentIndex = -1;
        }
    }
}