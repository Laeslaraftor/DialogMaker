using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# numbers converter
    /// </summary>
    public static unsafe class DSharpNumberConverter
    {
        private static readonly Dictionary<Type, ConverterPointer> _converters = new()
        {
            [typeof(byte)] = new(&DSharpNumber.ByteConverter),
            [typeof(sbyte)] = new(&DSharpNumber.SByteConverter),
            [typeof(bool)] = new(&DSharpNumber.BoolConverter),
            [typeof(short)] = new(&DSharpNumber.ShortConverter),
            [typeof(ushort)] = new(&DSharpNumber.UShortConverter),
            [typeof(char)] = new(&DSharpNumber.CharConverter),
            [typeof(int)] = new(&DSharpNumber.IntConverter),
            [typeof(uint)] = new(&DSharpNumber.UIntConverter),
            [typeof(long)] = new(&DSharpNumber.LongConverter),
            [typeof(ulong)] = new(&DSharpNumber.ULongConverter),
            [typeof(nint)] = new(&DSharpNumber.NIntConverter),
            [typeof(nuint)] = new(&DSharpNumber.NUIntConverter),
            [typeof(float)] = new(&DSharpNumber.FloatConverter),
            [typeof(double)] = new(&DSharpNumber.DoubleConverter),
            [typeof(decimal)] = new(&DSharpNumber.DecimalConverter),
        };

        /// <summary>
        /// Convert provided number to specified type
        /// </summary>
        /// <typeparam name="T">Destination type</typeparam>
        /// <param name="number">Number that need to be converted</param>
        /// <returns>Converted value</returns>
        public static T Convert<T>(in DSharpNumber number) where T : unmanaged
        {
            var size = sizeof(T);
            byte* buffer = stackalloc byte[size];

            Converter<T>.Convert(number, buffer);

            return Unsafe.ReadUnaligned<T>(buffer);
        }

        #region Types

        private readonly struct ConverterPointer(delegate*<in DSharpNumber, void*, void> value)
        {
            public readonly delegate*<in DSharpNumber, void*, void> Pointer = value;
        }
        private static class Converter<T> where T : unmanaged
        {
            static Converter()
            {
                _converter = _converters[typeof(T)].Pointer;
            }

            private static readonly delegate*<in DSharpNumber, void*, void> _converter;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Convert(in DSharpNumber number, void* resultBuffer)
            {
                _converter(number, resultBuffer);
            }
        }

        #endregion
    }
}
