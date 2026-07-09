using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Class with constants of build-in types information
    /// </summary>
    public static class DSharpBuildInTypes
    {
        /// <summary>
        /// Class with extra build-in types
        /// </summary>
        public static class Extra
        {
            /// <summary>
            /// Class with information about type
            /// </summary>
            public static readonly DSharpBuildInTypeInfo Type = new("System.Type");
            /// <summary>
            /// Types that was base of all arrays
            /// </summary>
            public static readonly DSharpBuildInTypeInfo Array = new("System.Array`1");
            /// <summary>
            /// Base type of all enums
            /// </summary>
            public static readonly DSharpBuildInTypeInfo Enum = new("System.Enum");
            /// <summary>
            /// Standard exception type
            /// </summary>
            public static readonly DSharpBuildInTypeInfo Exception = new("System.Exception");
            /// <summary>
            /// Standard enumerator interface
            /// </summary>
            public static readonly DSharpBuildInTypeInfo IEnumerator = new("System.Collections.IEnumerator");
        }

        /// <summary>
        /// Boolean/bool: true, false (1 byte)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Boolean = new("System.Boolean", sizeof(bool), DSharpTokenType.Bool);
        /// <summary>
        /// Unsigned byte: 0-255 (1 byte)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Byte = new("System.Byte", sizeof(byte), DSharpTokenType.Byte);
        /// <summary>
        /// Signed byte: -128-127 (1 byte)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo SignedByte = new("System.SByte", sizeof(sbyte), DSharpTokenType.SByte);
        /// <summary>
        /// UTF-16 character is unsigned short (2 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Char = new("System.Char", sizeof(char), DSharpTokenType.Char);
        /// <summary>
        /// Decimal: ±1.0 x 10^-28 to ±7.9228 x 10^28, 28-29 digits (16 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Decimal = new("System.Decimal", sizeof(decimal), DSharpTokenType.Decimal);
        /// <summary>
        /// Double precision floating-point number: ±5.0 × 10^−324 to ±1.7 × 10^308, ~15-17 digits (8 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Double = new("System.Double", sizeof(double), DSharpTokenType.Double);
        /// <summary>
        /// Single precision floating-point number: ±1.5 x 10^−45 to ±3.4 x 10^38, ~6-9 digits (4 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Single = new("System.Single", sizeof(float), DSharpTokenType.Float);
        /// <summary>
        /// Integer: -2,147,483,648 to 2,147,483,647 (4 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Int = new("System.Int32", sizeof(int), DSharpTokenType.Int);
        /// <summary>
        /// Unsigned integer: 0 to 4,294,967,295 (4 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo UnsignedInt = new("System.UInt32", sizeof(uint), DSharpTokenType.UInt);
        /// <summary>
        /// Native integer which size depends on platform (4 or 8 bytes)
        /// </summary>
        public static unsafe readonly DSharpBuildInTypeInfo NativeInt = new("System.IntPtr", sizeof(nint), DSharpTokenType.Nint);
        /// <summary>
        /// Unsigned native integer which size depends on platform (4 or 8 bytes)
        /// </summary>
        public static unsafe readonly DSharpBuildInTypeInfo NativeUnsignedInt = new("System.UIntPtr", sizeof(nuint), DSharpTokenType.Nuint);
        /// <summary>
        /// Long: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807 (8 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Long = new("System.Int64", sizeof(long), DSharpTokenType.Long);
        /// <summary>
        /// Unsigned long: 0 to 18,446,744,073,709,551,615 (8 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo UnsignedLong = new("System.UInt64", sizeof(ulong), DSharpTokenType.ULong);
        /// <summary>
        /// Short integer: -32,768 to 32,767 (2 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Short = new("System.Int16", sizeof(short), DSharpTokenType.Short);
        /// <summary>
        /// Unsigned short integer: 0 to 65,535 (2 bytes)
        /// </summary>
        public static readonly DSharpBuildInTypeInfo UnsignedShort = new("System.UInt16", sizeof(ushort), DSharpTokenType.UShort);
        /// <summary>
        /// Empty structure that represents non returning value
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Void = new("System.Void", 0, DSharpTokenType.Void);
        /// <summary>
        /// Object type which root of all objects
        /// </summary>
        public static readonly DSharpBuildInTypeInfo Object = new("System.Object", -1, DSharpTokenType.Object);
        /// <summary>
        /// UTF-16 string
        /// </summary>
        public static readonly DSharpBuildInTypeInfo String = new("System.String", -1, DSharpTokenType.String);

        /// <summary>
        /// Dictionary of all build-in value types
        /// </summary>
        public static ReadOnlyDictionary<string, DSharpBuildInTypeInfo> AllValueTypes
        {
            get
            {
                if (field == null)
                {
                    Dictionary<string, DSharpBuildInTypeInfo> values = new()
                    {
                        { Int.FullName, Int },
                        { UnsignedInt.FullName, UnsignedInt },
                        { Long.FullName, Long },
                        { UnsignedLong.FullName, UnsignedLong },
                        { Short.FullName, Short },
                        { UnsignedShort.FullName, UnsignedShort },
                        { NativeInt.FullName, NativeInt },
                        { NativeUnsignedInt.FullName, NativeUnsignedInt },
                        { Byte.FullName, Byte },
                        { SignedByte.FullName, SignedByte },
                        { Boolean.FullName, Boolean },
                        { Char.FullName, Char },
                        { Decimal.FullName, Decimal },
                        { Double.FullName, Double },
                        { Single.FullName, Single },
                        { Void.FullName, Void },
                    };

                    field = new(values);
                }

                return field;
            }
        }
        /// <summary>
        /// Dictionary of all build-in types
        /// </summary>
        public static ReadOnlyDictionary<string, DSharpBuildInTypeInfo> AllTypes
        {
            get
            {
                if (field == null)
                {
                    Dictionary<string, DSharpBuildInTypeInfo> values = new()
                    {
                        { Int.FullName, Int },
                        { UnsignedInt.FullName, UnsignedInt },
                        { Long.FullName, Long },
                        { UnsignedLong.FullName, UnsignedLong },
                        { Short.FullName, Short },
                        { UnsignedShort.FullName, UnsignedShort },
                        { NativeInt.FullName, NativeInt },
                        { NativeUnsignedInt.FullName, NativeUnsignedInt },
                        { Byte.FullName, Byte },
                        { SignedByte.FullName, SignedByte },
                        { Boolean.FullName, Boolean },
                        { Char.FullName, Char },
                        { Decimal.FullName, Decimal },
                        { Double.FullName, Double },
                        { Single.FullName, Single },
                        { Object.FullName, Object },
                        { String.FullName, String },
                        { Void.FullName, Void },
                    };

                    field = new(values);
                }

                return field;
            }
        }

        /// <summary>
        /// Try get build-in type by token
        /// </summary>
        /// <param name="assembly">Assembly for searching type</param>
        /// <param name="standardTypeToken">Token that represents type</param>
        /// <param name="result">Type that found</param>
        /// <returns>Is type found</returns>
        public static bool TryGetType(IDSharpAssembly assembly, DSharpTokenType standardTypeToken, [NotNullWhen(true)] out IDSharpType? result)
        {
            result = null;

            foreach (var info in AllTypes.Values)
            {
                if (info.Token == standardTypeToken)
                {
                    result = assembly.GetType(info.FullName);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Try get information about standard type by name
        /// </summary>
        /// <param name="name">Short or full name of type</param>
        /// <param name="result">Information about type</param>
        /// <returns>Is information found</returns>
        public static bool TryGetTypeInfo(string name, [NotNullWhen(true)] out DSharpBuildInTypeInfo result)
        {
            result = default;

            foreach (var info in AllTypes.Values)
            {
                if (info.ShortName == name || info.FullName == name)
                {
                    result = info;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Try get information about standard type
        /// </summary>
        /// <param name="type">Standard type</param>
        /// <param name="result">Information about type</param>
        /// <returns>Is information found</returns>
        public static bool TryGetInfo(IDSharpType type, [NotNullWhen(true)] out DSharpBuildInTypeInfo result)
        {
            return TryGetTypeInfo(type.FullName, out result);
        }
        /// <summary>
        /// Try get information about standard type by literal type
        /// </summary>
        /// <param name="type">Literal type</param>
        /// <param name="result">Information about type</param>
        /// <returns>Is information found</returns>
        public static bool TryGetTypeInfo(DSharpLiteralType type, [NotNullWhen(true)] out DSharpBuildInTypeInfo result)
        {
            var token = (DSharpTokenType)type;

            foreach (var info in AllTypes.Values)
            {
                if (info.Token == token)
                {
                    result = info;
                    return true;
                }
            }

            result = default;
            return false;
        }
        /// <summary>
        /// Try get value type by it's index in <see cref="AllValueTypes"/>
        /// </summary>
        /// <param name="index">Value type index</param>
        /// <param name="result">Information about type</param>
        /// <returns>Is type information found</returns>
        public static bool TryGetValueTypeByIndex(int index, [NotNullWhen(true)] out DSharpBuildInTypeInfo result)
        {
            int i = 0;

            foreach (var info in AllValueTypes.Values)
            {
                if (i == index)
                {
                    result = info;
                    return true;
                }
            }

            result = default;
            return false;
        }
        /// <summary>
        /// Try get value type index from <see cref="AllValueTypes"/>
        /// </summary>
        /// <param name="type">Type to search index</param>
        /// <param name="result">Type index</param>
        /// <returns>Is index found</returns>
        public static bool TryGetValueTypeIndex(IDSharpType type, out int result)
        {
            result = 0;
            var fullName = type.FullName;

            foreach (var info in AllValueTypes.Values)
            {
                if (info.FullName == fullName)
                {
                    return true;
                }

                result++;
            }

            result = -1;
            return false;
        }

        /// <summary>
        /// Check type is point-floating
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Is specified type point-floating</returns>
        public static bool IsPointFloating(this DSharpBuildInTypeInfo type)
        {
            return type == Single ||
                   type == Double ||
                   type == Decimal;
        }
        /// <summary>
        /// Check type is unsigned
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Is specified type unsigned</returns>
        public static bool IsUnsigned(this DSharpBuildInTypeInfo type)
        {
            return type == Byte ||
                   type == UnsignedInt ||
                   type == UnsignedLong ||
                   type == UnsignedShort ||
                   type == NativeUnsignedInt;
        }
        /// <summary>
        /// Check type is number
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Is specified type number</returns>
        public static bool IsNumber(this DSharpBuildInTypeInfo type)
        {
            return type.Size > 0 && type != Boolean;
        }
        /// <summary>
        /// Check type is number
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Is specified type number</returns>
        public static bool IsNumber(this IDSharpType type)
        {
            if (TryGetInfo(type, out var info))
            {
                return info.IsNumber();
            }

            return false;
        }
        /// <summary>
        /// Check is target build-in type can be casted to destination build-in type
        /// </summary>
        /// <param name="target">Type to casting</param>
        /// <param name="destination">Cast destination type</param>
        /// <returns>Is target type can be casted to destination type</returns>
        public static DSharpCastAvailability CanCast(this DSharpBuildInTypeInfo target, DSharpBuildInTypeInfo destination)
        {
            if (target == destination)
            {
                return DSharpCastAvailability.Implicit;
            }
            if (!IsNumber(target) || !IsNumber(destination))
            {
                return DSharpCastAvailability.No;
            }
            if (IsPointFloating(target) && !IsPointFloating(destination) ||
                !IsPointFloating(target) && IsPointFloating(destination) ||
                IsUnsigned(target) && !IsUnsigned(destination) ||
                !IsUnsigned(target) && IsUnsigned(destination) ||
                destination.Size > target.Size)
            {
                return DSharpCastAvailability.Explicit;
            }

            return DSharpCastAvailability.Implicit;
        }
    }
}
