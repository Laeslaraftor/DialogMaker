using System.Collections;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Array in unmanaged memory
    /// </summary>
    /// <typeparam name="T">Items type</typeparam>
    /// <param name="items">Unmanaged array</param>
    /// <param name="length">Length of array</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct UnmanagedArray<T>(T* items, int length) : IEnumerable<T>
        where T : unmanaged
    {
        /// <summary>
        /// Length of array
        /// </summary>
        public int Length => _length;
        /// <summary>
        /// Get or set item on specified index
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Item on specified index</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T this[int index]
        {
            get
            {
                if (0 > index || index >= _length)
                {
                    throw new IndexOutOfRangeException();
                }

                return _items[index];
            }
            set
            {
                if (0 > index || index >= _length)
                {
                    throw new IndexOutOfRangeException();
                }

                _items[index] = value;
            }
        }

        private readonly int _length = length;
        private readonly T* _items = items;

        #region Controls

        /// <summary>
        /// Get reference to item on specified index
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Reference to item on specified index</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T* GetItemReference(int index)
        {
            if (0 > index || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            return &_items[index];
        }

        /// <summary>
        /// Get index of specified item
        /// </summary>
        /// <param name="item">Item to find it index</param>
        /// <returns>Item index</returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < _length; i++)
            {
                if (_items[i].Equals(item))
                {
                    return i;
                }
            }

            return -1;
        }
        /// <summary>
        /// Check specified item on containing in array
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>Is item contains in array</returns>
        public bool Contains(T item) => IndexOf(item) != -1;

        public static implicit operator ReadOnlySpan<T>(UnmanagedArray<T> array) => new(array._items, array._length);

        #endregion

        #region Enumeration

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _length; i++)
            {
                yield return this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Static

        /// <summary>
        /// Create array in unmanaged memory
        /// </summary>
        /// <param name="length">Length of array</param>
        /// <returns>Unmanaged array</returns>
        public static UnmanagedArray<T> Create(int length)
        {
            int size = sizeof(T) * length;
            T* items = (T*)Marshal.AllocHGlobal(size);

            return new(items, size);
        }

        #endregion
    }
}
