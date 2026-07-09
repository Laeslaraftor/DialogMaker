namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    internal unsafe class MemoryBuilder(nint pointer, int size)
    {
        public nint Pointer { get; } = pointer;
        public int Size { get; } = size;
        public nint CurrentAddress { get; private set; } = pointer;
        public uint Allocated { get; private set; }

        public nint Allocate(int size)
        {
            if (size + Allocated > Size)
            {
                throw new InvalidOperationException("Not enough memory");
            }

            Allocated += (uint)size;
            nint currentPointer = (nint)(Pointer + Allocated);
            CurrentAddress = currentPointer;

            return currentPointer;
        }
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
    }
}
