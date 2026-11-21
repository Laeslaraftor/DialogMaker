using System.Native;
using System;

namespace Internal.System.Runtime;

internal static unsafe class CompilerServices
{
    public static extern nint GetObjectAddress(object obj);
    public static extern TValue* GetObjectContentAddress<T, TValue>(T obj);
    public static RuntimeTypeInfo* GetObjectTypeToken(object obj)
    {
        var address = GetObjectAddress(obj);

        if (address == 0)
        {
            return null;
        }

        return *(RuntimeTypeInfo**)address;
    }
}