using System;
using System.Native;

namespace Internal.System.Runtime;

internal static unsafe class RuntimeHelper
{
    public static Type CreateType(nint token)
    {
        Pointer<RuntimeTypeInfo> typePointer = new(token);
        var typeInfo = typePointer[0];

        return new Type(typeInfo);
    }
    public static void ThrowExecutionEngineException(string message)
    {
        throw new ExecutionEngineException(message);
    }
}