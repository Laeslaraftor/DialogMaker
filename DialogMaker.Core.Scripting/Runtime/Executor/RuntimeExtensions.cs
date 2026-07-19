using System.Numerics;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Class with extensions for D# runtime
    /// </summary>
    public static unsafe class RuntimeExtensions
    {
        extension(DSharpVmMemoryManager memoryManager)
        {
            /// <summary>
            /// <inheritdoc cref="DSharpVmMemoryManager.Free(nint)"/>
            /// </summary>
            /// <param name="pointer">Pointer to memory that need to free</param>
            /// <returns><inheritdoc cref="DSharpVmMemoryManager.Free(nint)"/></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Free(void* pointer) => memoryManager.Free((nint)pointer);
            /// <summary>
            /// Allocate memory for structure with extra size
            /// </summary>
            /// <typeparam name="T">Type of structure</typeparam>
            /// <param name="type">Type of memory for allocation</param>
            /// <param name="extraSize">Extra size that will be added to structure size</param>
            /// <returns>Pointer to allocated memory</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T* Allocate<T>(DSharpMemoryBlockType type, int extraSize = 0) where T : unmanaged
            {
                return (T*)memoryManager.Allocate(type, sizeof(T) + extraSize);
            }
        }

        /// <summary>
        /// Check specified is buffer empty (all values is 0)
        /// </summary>
        /// <param name="buffer">Buffer to check</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Is buffer empty</returns>
        public static bool IsEmpty(void* buffer, int bufferSize)
        {
            if (0 >= bufferSize)
            {
                return true;
            }

            var pointer = (byte*)buffer;
            int vectorSize = Vector<byte>.Count;
            int vectorCount = bufferSize / vectorSize;
            Vector<byte> zeroVector = Vector<byte>.Zero;

            for (int i = 0; i < vectorCount; i++)
            {
                Vector<byte> vector = Unsafe.ReadUnaligned<Vector<byte>>(pointer + i * vectorSize);

                if (!Vector.EqualsAll(vector, zeroVector))
                {
                    return false;
                }
            }

            pointer += vectorCount * vectorSize;
            int remaining = bufferSize % vectorSize;
            int longCount = remaining / 8;
            long* longPointer = (long*)pointer;

            for (int i = 0; i < longCount; i++)
            {
                if (longPointer[i] != 0)
                {
                    return false;
                }
            }

            pointer += longCount * 8;
            remaining %= 8;

            for (int i = 0; i < remaining; i++)
            {
                if (pointer[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Fill specified buffer with zeroes
        /// </summary>
        /// <param name="buffer">Buffer to filling</param>
        /// <param name="bufferSize">Buffer size</param>
        public static void FillZero(void* buffer, int bufferSize)
        {
            Span<byte> span = new(buffer, bufferSize);
            span.Clear();
        }
    }
}
