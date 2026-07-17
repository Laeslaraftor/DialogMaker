using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// List that works with unmanaged array
    /// </summary>
    /// <typeparam name="T">Type of element</typeparam>
    /// <param name="array">Items array</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct UnmanagedList<T>(UnmanagedArray<T> array) where T : unmanaged
    {
        /// <summary>
        /// Amount of items is list
        /// </summary>
        public readonly int Count => _count;
        /// <summary>
        /// List capacity
        /// </summary>
        public readonly int Capacity => _array.Length;
        /// <summary>
        /// Get or set item on specified index
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Item on specified index</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public readonly T this[int index]
        {
            get
            {
                if (0 > index || index >= _count)
                {
                    throw new IndexOutOfRangeException();
                }

                return _array[index];
            }
            set
            {
                if (0 > index || index >= _count)
                {
                    throw new IndexOutOfRangeException();
                }

                _array[index] = value;
            }
        }

        private readonly UnmanagedArray<T> _array = array;
        private int _count;

        /// <summary>
        /// Get index of specified item
        /// </summary>
        /// <param name="item">Item to search it index</param>
        /// <returns>Item index, if item not found returns -1</returns>
        public readonly int IndexOf(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_array[i].Equals(item))
                {
                    return i;
                }
            }

            return -1;
        }
        /// <summary>
        /// Check item containing is list
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>Is item contains in list</returns>
        public readonly bool Contains(T item) => IndexOf(item) != -1;
        /// <summary>
        /// Add item to end of list
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>Is item added</returns>
        public bool Add(T item)
        {
            if (_count + 1 >= _array.Length)
            {
                return false;
            }

            _array[_count] = item;
            _count++;

            return true;
        }
        /// <summary>
        /// Remove item from list
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>Is item removed</returns>
        public bool Remove(T item)
        {
            bool isFound = false;

            for (int i = 0; i < _count; i++)
            {
                if (isFound)
                {
                    _array[i - 1] = _array[i];
                    continue;
                }
                if (_array[i].Equals(item))
                {
                    isFound = true;
                }
            }

            if (isFound)
            {
                _count--;
            }

            return isFound;
        }
        /// <summary>
        /// Clear list
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _count; i++)
            {
                _array[i] = default;
            }

            _count = 0;
        }

        /// <summary>
        /// Convert unmanaged array to unmanaged list
        /// </summary>
        /// <param name="array">Array to converting</param>
        public static implicit operator UnmanagedList<T>(UnmanagedArray<T> array) => new(array);
        /// <summary>
        /// Convert unmanaged list to unmanaged array
        /// </summary>
        /// <param name="list">List to converting</param>
        public static implicit operator UnmanagedArray<T>(UnmanagedList<T> list) => list._array;
    }
}
