using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Delegate that represents external method from D#
    /// </summary>
    /// <param name="instance">Object instance that called method. This parameter <c>null</c> when method is static</param>
    /// <param name="methodInfo">Called method information</param>
    /// <param name="genericParameters">Calling generic parameter</param>
    /// <param name="arguments">Calling parameters</param>
    /// <returns>
    /// Value that will be added to stack after executing method.
    /// If you don't want return anything then return <c>null</c>.
    /// If you want to return D# null value, use <see cref="DSharpExternalMethodResult.Null"/>
    /// </returns>
    public unsafe delegate DSharpExternalMethodResult? DSharpExternalMethod(DSharpObject* instance, 
                                                                            DSharpRuntimeMethodInfo* methodInfo,
                                                                            UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters, 
                                                                            UnmanagedArray<DSharpExecutionLocalVariable> arguments);
}
