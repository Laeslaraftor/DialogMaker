namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Attribute that sets D# operation executor
    /// </summary>
    /// <param name="type">Type of D# operation executor</param>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ExecutorAttribute(Type type) : Attribute
    {
        /// <summary>
        /// Type of D# operation executor
        /// </summary>
        public Type ExecutorType { get; } = type;
    }
}
