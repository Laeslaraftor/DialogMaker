namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Attributes that sets amount of arguments
    /// </summary>
    /// <param name="count">Amount of arguments</param>
    public sealed class ArgsCountAttribute(int count) : Attribute
    {
        /// <summary>
        /// Amount of arguments
        /// </summary>
        public int Count { get; } = count;
    }
}
