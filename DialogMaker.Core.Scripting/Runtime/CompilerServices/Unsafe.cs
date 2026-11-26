#if NETSTANDARD2_1_OR_GREATER

namespace System.Runtime.CompilerServices
{
    internal static unsafe class Unsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(void* destination, T value) 
            where T : unmanaged
        {
            *(T*)destination = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(void* destination, T value) 
            where T : unmanaged
        {
            *(T*)destination = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(void* source)
            where T : unmanaged
        {
            return *(T*)source;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            TTo* result = stackalloc TTo[1];
            WriteUnaligned(result, source);

            return ref *result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipInit<T>(out T value)
            where T : unmanaged
        {
            value = default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlockUnaligned(void* destination, void* source, uint byteCount)
        {
            Buffer.MemoryCopy(source, destination, byteCount, byteCount);
        }
    }
}
#endif