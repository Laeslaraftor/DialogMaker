using DialogMaker.Core.Scripting.Runtime.Executor;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Static class with methods for converting D# objects to C# objects
    /// </summary>
    public static unsafe class DSharpObjectConverter
    {
        /// <summary>
        /// Convert D# string to C# string
        /// </summary>
        /// <param name="obj">D# string instance</param>
        /// <returns>C# string instance</returns>
        public static string ToString(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.String);
            char* chars = DSharpObject.GetData<char>(obj);
            var size = DSharpObject.GetSize(obj);

            return new(chars, 0, size);
        }
        /// <summary>
        /// Convert D# byte to C# byte
        /// </summary>
        /// <param name="obj">D# byte</param>
        /// <returns>C# byte</returns>
        public static byte ToByte(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Byte);
            return *(byte*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# sbyte to C# sbyte
        /// </summary>
        /// <param name="obj">D# sbyte</param>
        /// <returns>C# sbyte</returns>
        public static sbyte ToSByte(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.SignedByte);
            return *(sbyte*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# short to C# short
        /// </summary>
        /// <param name="obj">D# short</param>
        /// <returns>C# short</returns>
        public static short ToInt16(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Short);
            return *(short*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# ushort to C# ushort
        /// </summary>
        /// <param name="obj">D# ushort</param>
        /// <returns>C# ushort</returns>
        public static ushort ToUInt16(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.UnsignedShort);
            return *(ushort*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# int to C# int
        /// </summary>
        /// <param name="obj">D# int</param>
        /// <returns>C# int</returns>
        public static int ToInt32(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Int);
            return *(int*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# uint to C# uint
        /// </summary>
        /// <param name="obj">D# uint</param>
        /// <returns>C# uint</returns>
        public static uint ToUInt32(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.UnsignedInt);
            return *(uint*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# long to C# long
        /// </summary>
        /// <param name="obj">D# long</param>
        /// <returns>C# long</returns>
        public static long ToInt64(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Long);
            return *(long*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# ulong to C# ulong
        /// </summary>
        /// <param name="obj">D# ulong</param>
        /// <returns>C# ulong</returns>
        public static ulong ToUInt64(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.UnsignedLong);
            return *(ulong*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# nint to C# nint
        /// </summary>
        /// <param name="obj">D# nint</param>
        /// <returns>C# nint</returns>
        public static nint ToIntPtr(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.NativeInt);
            return *(nint*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# nuint to C# nuint
        /// </summary>
        /// <param name="obj">D# nuint</param>
        /// <returns>C# nuint</returns>
        public static nuint ToUIntPtr(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.NativeUnsignedInt);
            return *(nuint*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# float to C# float
        /// </summary>
        /// <param name="obj">D# float</param>
        /// <returns>C# float</returns>
        public static float ToSingle(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Single);
            return *(float*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# double to C# double
        /// </summary>
        /// <param name="obj">D# double</param>
        /// <returns>C# double</returns>
        public static double ToDouble(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Double);
            return *(double*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# decimal to C# decimal
        /// </summary>
        /// <param name="obj">D# decimal</param>
        /// <returns>C# decimal</returns>
        public static decimal ToDecimal(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Decimal);
            return *(decimal*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# bool to C# bool
        /// </summary>
        /// <param name="obj">D# bool</param>
        /// <returns>C# bool</returns>
        public static bool ToBoolean(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Boolean);
            return *(bool*)DSharpObject.GetData(obj);
        }
        /// <summary>
        /// Convert D# char to C# char
        /// </summary>
        /// <param name="obj">D# char</param>
        /// <returns>C# char</returns>
        public static char ToChar(DSharpObject* obj)
        {
            CheckType(obj, DSharpBuildInTypes.Char);
            return *(char*)DSharpObject.GetData(obj);
        }

        /// <summary>
        /// Convert D# object to C# object
        /// </summary>
        /// <param name="obj">D# object instance</param>
        /// <returns>C# object</returns>
        /// <exception cref="InvalidOperationException">Object type has not converter</exception>
        public static T ToObject<T>(DSharpObject* obj)
        {
            var convertedValue = ToObject(obj);
            return (T)Convert.ChangeType(convertedValue, typeof(T));
        }
        /// <summary>
        /// Convert D# object to C# object
        /// </summary>
        /// <param name="obj">D# object instance</param>
        /// <returns>C# object</returns>
        /// <exception cref="InvalidOperationException">Object type has not converter</exception>
        public static object ToObject(DSharpObject* obj)
        {
            if (obj->Type->Converter == null)
            {
                throw new InvalidOperationException($"\"{*obj}\" has not converter");
            }

            return obj->Type->Converter(obj);
        }

        private static void CheckType(DSharpObject* obj, DSharpBuildInTypeInfo typeInfo)
        {
            if (CompareString(obj->Type->Namespace, typeInfo.Namespace) &&
                CompareString(obj->Type->Name, typeInfo.Name))
            {
                return;
            }

            throw new ArgumentException($"Provided object should be \"{typeInfo.FullName}\"");
        }
        private static bool CompareString(UnmanagedArray<char> chars, string str)
        {
            if (chars.Length == str.Length)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (chars[i] != str[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
