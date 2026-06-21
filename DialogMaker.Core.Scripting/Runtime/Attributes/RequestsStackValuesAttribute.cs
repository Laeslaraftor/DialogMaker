namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Attribute that sets requested amount of stack values
    /// </summary>
    /// <param name="count">Requested amount of stack values</param>
    public sealed class RequestsStackValuesAttribute(int count) : Attribute
    {
        /// <summary>
        /// Requested amount of stack values
        /// </summary>
        public int Count { get; } = count;
    }
}
