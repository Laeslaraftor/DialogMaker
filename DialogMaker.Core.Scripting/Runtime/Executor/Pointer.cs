using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Typed pointer
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="value">Pointer to object</param>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Pointer<T>(T* value) : IEquatable<Pointer<T>>
        where T : unmanaged
    {
        /// <summary>
        /// Create typed pointer
        /// </summary>
        /// <param name="pointer">Pointer to object</param>
        public Pointer(void* pointer) : this((T*)pointer)
        {
        }

        private readonly T* _value = value;

        /// <summary>
        /// Get pointer
        /// </summary>
        /// <returns>Pointer</returns>
        public T* AsPointer() => _value;

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Pointer<T> other && Equals(other);
        }
        public bool Equals(Pointer<T> other)
        {
            return _value == other._value;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine((nint)_value);
        }

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
        /// <summary>
        /// Convert unsafe pointer to typed pointer
        /// </summary>
        /// <param name="pointer">Unsafe pointer to convert</param>
        public static implicit operator Pointer<T>(void* pointer) => new(pointer);

        public static bool operator ==(Pointer<T> left, Pointer<T> right) => left.Equals(right);
        public static bool operator !=(Pointer<T> left, Pointer<T> right) => !left.Equals(right);
    }
}
