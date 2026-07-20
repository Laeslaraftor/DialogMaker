using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Pair of unmanaged key and value
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct UnmanagedPair<TKey, TValue>() : IEquatable<UnmanagedPair<TKey, TValue>>
        where TKey : unmanaged
        where TValue : unmanaged
    {
        /// <summary>
        /// Create new pair of unmanaged key and value
        /// </summary>
        /// <param name="key">Pair key</param>
        /// <param name="value">Pair value</param>
        public UnmanagedPair(TKey key, TValue value) : this()
        {
            Key = key; 
            Value = value;
        }

        /// <summary>
        /// Pair key
        /// </summary>
        public TKey Key;
        /// <summary>
        /// Pair value
        /// </summary>
        public TValue Value;

        public static bool operator ==(UnmanagedPair<TKey, TValue> l, UnmanagedPair<TKey, TValue> r) => l.Equals(r);
        public static bool operator !=(UnmanagedPair<TKey, TValue> l, UnmanagedPair<TKey, TValue> r) => !l.Equals(r);

        public readonly bool Equals(UnmanagedPair<TKey, TValue> other)
        {
            return Equals(Key, other.Key) && Equals(Value, other.Value);
        }
        public readonly override bool Equals(object? obj)
        {
            return obj is UnmanagedPair<TKey, TValue> other &&
                   Equals(other);
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Key, Value);
        }
        public readonly override string ToString()
        {
            return $"[{Key}, {Value}]";
        }
    }
}
