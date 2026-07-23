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
        /// Create unmanaged array
        /// </summary>
        /// <param name="items">Unmanaged array</param>
        /// <param name="length">Length of array</param>
        public UnmanagedArray(nint items, int length)
            : this((T*)items, length)
        {
        }

        /// <summary>
        /// Is items null pointer or length less or equals then 0;
        /// </summary>
        public bool IsNull => _items == null || _length <= 0;
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
        /// Get pointer to first item
        /// </summary>
        /// <returns>Pointer to first item</returns>
        public T* AsPointer() => _items;
        /// <summary>
        /// Get casted item on specified index
        /// </summary>
        /// <typeparam name="TCast">New item type</typeparam>
        /// <param name="index">Item index</param>
        /// <returns>Casted type on specified index</returns>
        public TCast Cast<TCast>(int index) where TCast : unmanaged
        {
            return *(TCast*)GetItemReference(index);
        }
        /// <summary>
        /// Fill array with specified value
        /// </summary>
        /// <param name="value">Value for filling array</param>
        public void Fill(T value)
        {
            if (0 > _length)
            {
                return;
            }

            for (int i = 0; i < _length; i++)
            {
                _items[i] = value;
            }
        }
        /// <summary>
        /// Get unmanaged stream from current array
        /// </summary>
        /// <returns>Unmanaged stream</returns>
        public UnmanagedStream ToStream() => ToStream(0, _length);
        /// <summary>
        /// Get unmanaged stream from current array
        /// </summary>
        /// <param name="startIndex">Stream start offset</param>
        /// <param name="length">Stream length</param>
        /// <returns>Unmanaged stream</returns>
        public UnmanagedStream ToStream(int startIndex, int length)
        {
            if (startIndex + length > _length)
            {
                throw new IndexOutOfRangeException();
            }

            var size = sizeof(T);
            return new((nint)_items + startIndex * size, length * size);
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
        public static implicit operator T*(UnmanagedArray<T> array) => array._items;

        public static bool operator ==(UnmanagedArray<T> array, string text)
        {
            if (array.Length != text.Length)
            {
                return false;
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (!Equals(text[i], array[i]))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool operator !=(UnmanagedArray<T> array, string text) => !(array == text);

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
    }
}
