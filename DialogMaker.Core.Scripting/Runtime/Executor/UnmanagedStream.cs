namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Stream of unmanaged memory
    /// </summary>
    /// <param name="pointer">Start pointer to memory block</param>
    /// <param name="length">Length of memory block</param>
    public struct UnmanagedStream(nint pointer, int length)
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

        /// <summary>
        /// Read current value and increase position by type size
        /// </summary>
        /// <typeparam name="T">Read type</typeparam>
        /// <returns>Value that was read</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public unsafe T Read<T>() where T : unmanaged
        {
            if (Position + 1 >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            T* items = (T*)(Pointer + Position);
            Position += sizeof(T);

            return *items;
        }
    }
}
