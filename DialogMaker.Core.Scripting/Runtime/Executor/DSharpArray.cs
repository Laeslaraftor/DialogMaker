using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# array
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpArray
    {
        /// <summary>
        /// Current D# object
        /// </summary>
        public DSharpObject Object;
        /// <summary>
        /// Size for managed data, this not includes size of DSharpObject structure size.
        /// </summary>
        public int Size;
        /// <summary>
        /// Length if type is array
        /// </summary>
        public int Length;

        public readonly override string ToString()
        {
            return $"{Object}: {Length}";
        }

        #region Static

        /// <summary>
        /// Get D# array length
        /// </summary>
        /// <param name="obj">D# array instance</param>
        /// <returns>Array length</returns>
        public static int GetLength(DSharpObject* obj)
        {
            if (obj->IsArray)
            {
                return ((DSharpArray*)obj)->Length;
            }

            return 0;
        }
        /// <summary>
        /// Get item data on specified index
        /// </summary>
        /// <param name="array">Array for getting item</param>
        /// <param name="index">Item index</param>
        /// <returns>Pointer to start of item data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* GetItem(DSharpArray* array, int index)
        {
            var itemSize = array->Size / array->Length;
            return DSharpObject.GetData((DSharpObject*)array) + itemSize * index;
        }
        /// <summary>
        /// Get item data on specified index
        /// </summary>
        /// <param name="array">Array for getting item</param>
        /// <param name="index">Item index</param>
        /// <returns>Pointer to start of item data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetItem<T>(DSharpArray* array, int index) where T : unmanaged
        {
            return (T*)GetItem(array, index);
        }
        /// <summary>
        /// Get D# array indexer
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="array">D# array</param>
        /// <returns>D# array indexer</returns>
        public static Indexer<T> GetIndexer<T>(DSharpArray* array)
            where T : unmanaged
        {
            var itemSize = array->Size / array->Length;
            var data = DSharpObject.GetData((DSharpObject*)array);
            return new(data, itemSize, array->Length);
        }

        #endregion

        #region Structs

        /// <summary>
        /// D# array indexer
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="data">Pointer to array data</param>
        /// <param name="itemSize">Array item size</param>
        /// <param name="length">Array length</param>
        public readonly struct Indexer<T>(byte* data, int itemSize, int length) : IEnumerable<T>
            where T : unmanaged
        {
            /// <summary>
            /// Array length
            /// </summary>
            public int Length => _length;
            /// <summary>
            /// Item getter
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

                    return *GetPointer(index);
                }
            }

            private readonly byte* _data = data;
            private readonly int _itemSize = itemSize;
            private readonly int _length = length;

            #region Controls

            /// <summary>
            /// Get pointer to item on specified index
            /// </summary>
            /// <param name="index">Item index</param>
            /// <returns>Pointer to item</returns>
            /// <exception cref="IndexOutOfRangeException"></exception>
            public T* GetPointer(int index)
            {
                if (0 > index || index >= _length)
                {
                    throw new IndexOutOfRangeException();
                }

                return (T*)(_data + _itemSize * index);
            }

            #endregion

            #region Enumerable

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

        #endregion
    }
}
