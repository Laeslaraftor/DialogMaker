namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Stream of unmanaged memory
    /// </summary>
    /// <param name="pointer">Start pointer to memory block</param>
    /// <param name="length">Length of memory block</param>
    public unsafe struct UnmanagedStream(nint pointer, int length)
    {
        /// <summary>
        /// Start pointer to memory block
        /// </summary>
        public nint Pointer { get; } = pointer;
        /// <summary>
        /// Length of memory block
        /// </summary>
        public int Length { get; } = length;
        /// <summary>
        /// Current position in bytes
        /// </summary>
        public int Position { get; set; }

        #region Controls

        /// <summary>
        /// Read current value and increase position by type size
        /// </summary>
        /// <typeparam name="T">Read type</typeparam>
        /// <returns>Value that was read</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T Read<T>() where T : unmanaged
        {
            int size = sizeof(T);

            if (Position + size >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            T* items = (T*)(Pointer + Position);
            Position += size;

            return *items;
        }
        /// <summary>
        /// Read unmanaged array and increase position by array size
        /// </summary>
        /// <typeparam name="T">Array item type</typeparam>
        /// <param name="length">Array length</param>
        /// <returns>Unmanaged array that read</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public UnmanagedArray<T> ReadArray<T>(int length)
            where T : unmanaged
        {
            if (length == 0)
            {
                return new(Pointer, 0);
            }

            int size = sizeof(T) * length;

            if (Position + size >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            T* items = (T*)(Pointer + Position);
            Position += size;

            return new(items, length);
        }

        #endregion
    }
}
