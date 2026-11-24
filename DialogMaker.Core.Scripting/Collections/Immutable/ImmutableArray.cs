#if NETSTANDARD2_1_OR_GREATER

namespace System.Collections.Immutable
{
    public readonly struct ImmutableArray
    {
        public static ImmutableArray<T> Create<T>(params T[] items) => new(items);
    }
    public readonly struct ImmutableArray<T>(T[] items) : IEnumerable<T>
    {
        public T this[int index] => _items[index];
        public int Length => _items.Length;

        private readonly T[] _items = items;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return item;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Static

        public static readonly ImmutableArray<T> Empty = new([]);

        #endregion
    }
}

#endif