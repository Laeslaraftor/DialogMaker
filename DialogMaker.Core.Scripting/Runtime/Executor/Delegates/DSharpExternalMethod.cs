namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Delegate that represents external method from D#
    /// </summary>
    /// <param name="args">External method calling args</param>
    /// <returns>
    /// Value that will be added to stack after executing method.
    /// If you don't want return anything then return <c>null</c>.
    /// If you want to return D# null value, use <see cref="DSharpExternalMethodResult.Null"/>.
    /// If you manually push value into stack then use <see cref="DSharpExternalMethodResult.Stack"/>
    /// </returns>
    public unsafe delegate DSharpExternalMethodResult? DSharpExternalMethod(DSharpExternalCallingArgs args);
}
