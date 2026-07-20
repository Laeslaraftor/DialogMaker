namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Memory builder for creating complex structures from memory block
    /// </summary>
    /// <param name="pointer">Pointer to start of memory block</param>
    /// <param name="size">Size of memory block in bytes</param>
    public unsafe ref struct MemoryBuilder(nint pointer, int size)
    {
        /// <summary>
        /// Pointer to start of memory block
        /// </summary>
        public nint Pointer { get; } = pointer;
        /// <summary>
        /// Size of memory block
        /// </summary>
        public int Size { get; } = size;
        /// <summary>
        /// Current address in memory block
        /// </summary>
        public nint CurrentAddress { get; private set; } = pointer;
        /// <summary>
        /// Amount of allocated memory in bytes (offset relative to pointer)
        /// </summary>
        public uint Allocated { get; private set; }

        /// <summary>
        /// Allocate specified amount of bytes
        /// </summary>
        /// <param name="size">Amount of bytes to allocate</param>
        /// <returns>Address to start of allocated block</returns>
        /// <exception cref="InvalidOperationException">Not enough memory</exception>
        public nint Allocate(int size)
        {
            if (size + Allocated > Size)
            {
                throw new InvalidOperationException("Not enough memory");
            }

            nint currentPointer = (nint)(Pointer + Allocated);
            CurrentAddress = currentPointer;
            Allocated += (uint)size;

            return currentPointer;
        }
        /// <summary>
        /// Allocate array of items with specified length
        /// </summary>
        /// <typeparam name="T">Type of array element</typeparam>
        /// <param name="length">Array length</param>
        /// <returns>Allocated array</returns>
        public UnmanagedArray<T> AllocateArray<T>(int length)
            where T : unmanaged
        {
            if (length == 0)
            {
                return new(null, 0);
            }

            T* memory = (T*)Allocate(sizeof(T) * length);
            return new(memory, length);
        }
        /// <summary>
        /// Allocate string
        /// </summary>
        /// <param name="str">String to allocation</param>
        /// <returns>Allocated string</returns>
        public UnmanagedArray<char> AllocateString(string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new(null, 0);
            }

            var result = AllocateArray<char>(str.Length);

            for (int i = 0; i < str.Length; i++)
            {
                result[i] = str[i];
            }

            return result;
        }
        /// <summary>
        /// Allocate array from managed array
        /// </summary>
        /// <typeparam name="T">Array element type</typeparam>
        /// <param name="array">Managed array that will be used for creating and filling allocated array</param>
        /// <returns>Allocated array with items from managed array</returns>
        public UnmanagedArray<T> AllocateArray<T>(T[] array)
            where T : unmanaged
        {
            if (array.Length == 0)
            {
                return new(null, 0);
            }

            var result = AllocateArray<T>(array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }

            return result;
        }
        /// <summary>
        /// Allocate array from managed array with values converter
        /// </summary>
        /// <typeparam name="TSource">Source array element type</typeparam>
        /// <typeparam name="TDestination">Destination array element type</typeparam>
        /// <param name="array">Managed array that will be used for creating and filling allocated array</param>
        /// <param name="converter">Values converter</param>
        /// <returns>Allocated array with converted items from managed array</returns>
        public UnmanagedArray<TDestination> AllocateArray<TSource, TDestination>(TSource[] array, Func<TSource, TDestination> converter)
            where TDestination : unmanaged
        {
            if (array.Length == 0)
            {
                return new(null, 0);
            }

            var result = AllocateArray<TDestination>(array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                result[i] = converter(array[i]);
            }

            return result;
        }
        /// <summary>
        /// Allocate dictionary with specified capacity
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="capacity">Dictionary capacity for allocation</param>
        /// <returns>Allocated unmanaged dictionary</returns>
        public UnmanagedDictionary<TKey, TValue> AllocateDictionary<TKey, TValue>(int capacity)
            where TKey : unmanaged
            where TValue : unmanaged
        {
            var buffer = AllocateArray<UnmanagedPair<TKey, TValue>>(capacity);
            return new(buffer);
        }
    }
}
