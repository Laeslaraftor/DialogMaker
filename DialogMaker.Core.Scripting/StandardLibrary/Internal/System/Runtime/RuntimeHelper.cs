namespace Internal.System.Runtime;

using System;

internal static class RuntimeHelper
{
    public static Type CreateType(nint token)
    {
        Console.WriteLine("Creating type with address: " + token);
        throw new NotImplementedException();
    }
    public static void ThrowExecutionEngineException(string message)
    {
        throw new ExecutionEngineException(message);
    }
}