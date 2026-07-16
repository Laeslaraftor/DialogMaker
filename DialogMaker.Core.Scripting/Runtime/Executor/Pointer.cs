using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Typed pointer
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="value">Pointer to object</param>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Pointer<T>(T* value) where T : unmanaged
    {
        private readonly T* _value = value;

        /// <summary>
        /// Convert typed pointer to unsafe pointer
        /// </summary>
        /// <param name="pointer">Typed pointer to convert</param>
        public static implicit operator T*(Pointer<T> pointer) => pointer._value;
        /// <summary>
        /// Convert unsafe pointer to typed pointer
        /// </summary>
        /// <param name="pointer">Unsafe pointer to convert</param>
        public static implicit operator Pointer<T>(T* pointer) => new(pointer);
    }
}
