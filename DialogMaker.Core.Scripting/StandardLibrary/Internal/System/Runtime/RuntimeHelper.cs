using System;
using System.Native;

namespace Internal.System.Runtime;

internal static unsafe class RuntimeHelper
{
    public static Type CreateType(nint token)
    {
        return new Type(*(RuntimeTypeInfo*)token);
    }
    public static void ThrowExecutionEngineException(string message)
    {
        throw new ExecutionEngineException(message);
    }
    public static void ThrowDivideByZeroException()
    {
        throw new DivideByZeroException();
    }
}