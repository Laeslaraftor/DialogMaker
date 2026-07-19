namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Type of value that stores in stack
    /// </summary>
    public enum DSharpStackValueType : byte
    {
        Null,
        Structure,
        Reference,
        MethodCallingInfo,
        MethodParametersBuffer,
        Scope
    }
}
