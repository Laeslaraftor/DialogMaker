using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Class with extensions for D# runtime
    /// </summary>
    public static class RuntimeExtensions
    {
        extension(DSharpVmMemoryManager memoryManager)
        {
            /// <summary>
            /// <inheritdoc cref="DSharpVmMemoryManager.Free(nint)"/>
            /// </summary>
            /// <param name="pointer">Pointer to memory that need to free</param>
            /// <returns><inheritdoc cref="DSharpVmMemoryManager.Free(nint)"/></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool Free(void* pointer) => memoryManager.Free((nint)pointer);
            /// <summary>
            /// Allocate memory for structure with extra size
            /// </summary>
            /// <typeparam name="T">Type of structure</typeparam>
            /// <param name="type">Type of memory for allocation</param>
            /// <param name="extraSize">Extra size that will be added to structure size</param>
            /// <returns>Pointer to allocated memory</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe T* Allocate<T>(DSharpMemoryBlockType type, int extraSize = 0) where T : unmanaged
            {
                return (T*)memoryManager.Allocate(type, sizeof(T) + extraSize);
            }
        }
    }
}
