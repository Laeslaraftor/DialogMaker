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
        /// <returns>Pointer to value</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T* ReadPointer<T>() where T : unmanaged
        {
            int size = sizeof(T);

            if (Position + size >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            T* items = (T*)(Pointer + Position);
            Position += size;

            return items;
        }
        /// <summary>
        /// Read current value and increase position by type size
        /// </summary>
        /// <typeparam name="T">Read type</typeparam>
        /// <returns>Pointer to value</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public nint ReadSafePointer<T>() where T : unmanaged
        {
            return (nint)ReadPointer<T>();
        }
        /// <summary>
        /// Read current value and increase position by type size
        /// </summary>
        /// <typeparam name="T">Read type</typeparam>
        /// <returns>Value that was read</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T Read<T>() where T : unmanaged
        {
            return *ReadPointer<T>();
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
        /// <summary>
        /// Read values to buffer
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="buffer">Buffer for writing value from stream</param>
        /// <returns>Amount of values that was read</returns>
        public int Read<T>(Span<T> buffer)
            where T : unmanaged
        {
            int count = 0;
            var size = sizeof(T);

            while (Position < Length && count < buffer.Length)
            {
                buffer[count] = *(T*)(Pointer + Position);
                count++;
                Position += size;
            }

            return count;
        }

        /// <summary>
        /// Write value to stream
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="value">Value for writing to stream</param>
        /// <returns>Is value wrote</returns>
        public bool Write<T>(T value)
            where T : unmanaged
        {
            var size = sizeof(T);

            if (Position + size < Length)
            {
                *(T*)(Pointer + Position) = value;
                Position += size;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Write values to stream
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="buffer">Buffer for writing value to stream</param>
        /// <returns>Amount of values that was write</returns>
        public int WriteBuffer<T>(ReadOnlySpan<T> buffer)
            where T : unmanaged
        {
            int count = 0;
            var size = sizeof(T);

            while (Position + size < Length && count < buffer.Length)
            {
                *(T*)(Pointer + Position) = buffer[count];
                count++;
                Position += size;
            }

            return count;
        }

        #endregion
    }
}
