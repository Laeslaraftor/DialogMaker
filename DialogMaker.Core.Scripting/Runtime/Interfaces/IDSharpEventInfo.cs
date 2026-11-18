namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of D# event
    /// </summary>
    public interface IDSharpEventInfo : IDSharpMemberInfo
    {
        /// <summary>
        /// Type of event delegate
        /// </summary>
        public IDSharpType EventType { get; }
        /// <summary>
        /// Method for add event handler
        /// </summary>
        public IDSharpMethodInfo AddMethod { get; }
        /// <summary>
        /// Method for remove event handler
        /// </summary>
        public IDSharpMethodInfo RemoveMethod { get; }
        /// <summary>
        /// Field that contains event handlers.
        /// This field automatically created by compiler when add and remove method not implemented
        /// </summary>
        public IDSharpFieldInfo? HandlersField { get; }
    }
}
