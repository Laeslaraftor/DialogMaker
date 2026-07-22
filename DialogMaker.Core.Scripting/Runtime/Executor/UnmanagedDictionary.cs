using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Dictionary that works with unmanaged list
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <param name="buffer">Pairs buffer</param>
    public struct UnmanagedDictionary<TKey, TValue>(UnmanagedList<UnmanagedPair<TKey, TValue>> buffer)
        where TKey : unmanaged
        where TValue : unmanaged
    {
        /// <summary>
        /// Dictionary capacity
        /// </summary>
        public int Capacity => _pairs.Capacity;
        /// <summary>
        /// Current amount of pairs
        /// </summary>
        public int Count => _pairs.Count;
        /// <summary>
        /// Value getter/setter
        /// </summary>
        /// <param name="key">Key for get or set value</param>
        /// <returns>Value with specified key</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public TValue this[TKey key]
        {
            get
            {
                for (int i = 0; i < _pairs.Count; i++)
                {
                    var pair = _pairs[i];

                    if (Equals(pair.Key, key))
                    {
                        return pair.Value;
                    }
                }

                throw new KeyNotFoundException();
            }
            set
            {
                for (int i = 0; i < _pairs.Count; i++)
                {
                    var pair = _pairs[i];

                    if (Equals(pair.Key, key))
                    {
                        pair.Value = value;
                        _pairs[i] = pair;

                        return;
                    }
                }

                throw new KeyNotFoundException();
            }
        }
        /// <summary>
        /// Get pair by index
        /// </summary>
        /// <param name="index">Pair index</param>
        /// <returns>Pair on specified index</returns>
        public UnmanagedPair<TKey, TValue> this[int index] => _pairs[index];

        private UnmanagedList<UnmanagedPair<TKey, TValue>> _pairs = buffer;

        /// <summary>
        /// Check is specified key contains in dictionary
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>Is specified key contains in dictionary</returns>
        public bool ContainsKey(TKey key)
        {
            for (int i = 0; i < _pairs.Count; i++)
            {
                if (Equals(_pairs[i].Key, key))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Try to get value by key
        /// </summary>
        /// <param name="key">Key to search value</param>
        /// <param name="result">Value with same key</param>
        /// <returns>Is value found</returns>
        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? result)
        {
            for (int i = 0; i < _pairs.Count; i++)
            {
                var pair = _pairs[i];

                if (Equals(pair.Key, key))
                {
                    result = pair.Value;
                    return true;
                }
            }

            result = default;
            return false;
        }
        /// <summary>
        /// Add pair
        /// </summary>
        /// <param name="key">Pair key</param>
        /// <param name="value">Pair value</param>
        /// <exception cref="ArgumentException">Value with same key already added</exception>
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("Value with same key already added", nameof(key));
            }

            _pairs.Add(new(key, value));
        }
        /// <summary>
        /// Remove value with specified key
        /// </summary>
        /// <param name="key">Key for removing value</param>
        /// <returns>Is value removed</returns>
        public bool Remove(TKey key)
        {
            for (int i = 0; i < _pairs.Count; i++)
            {
                if (Equals(_pairs[i].Key, key))
                {
                    _pairs.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Clear dictionary
        /// </summary>
        public void Clear() => _pairs.Clear();
    }
}
