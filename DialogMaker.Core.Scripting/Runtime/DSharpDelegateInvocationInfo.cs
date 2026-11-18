namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about structure that contains invocation info
    /// </summary>
    public class DSharpDelegateInvocationInfo(IDSharpType type, IDSharpFieldInfo targetField, IDSharpFieldInfo runtimeMethodInfoField)
    {
        /// <summary>
        /// Invocation information structure type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Field that contains object invocation instance
        /// </summary>
        public IDSharpFieldInfo TargetField { get; } = targetField;
        /// <summary>
        /// Field that contains address to runtime method information
        /// </summary>
        public IDSharpFieldInfo RuntimeMethodInfoField { get; } = runtimeMethodInfoField;

        #region Static

        /// <summary>
        /// Create information about structure that contains invocation info
        /// </summary>
        /// <param name="assembly">Assembly for searching required type</param>
        /// <returns>Information about structure that contains invocation info</returns>
        public static DSharpDelegateInvocationInfo Create(IDSharpAssembly assembly)
        {
            return Create(assembly.GetType(DSharpBuildInTypes.Extra.DelegateInvocationInfo));
        }
        /// <summary>
        /// Create information about structure that contains invocation info
        /// </summary>
        /// <param name="type">Type that need to describe</param>
        /// <returns>Information about structure that contains invocation info</returns>
        public static DSharpDelegateInvocationInfo Create(IDSharpType type)
        {
            var targetField = type.GetField("Target");
            var runtimeMethodInfoField = type.GetField("RuntimeMethodInfo");

            return new(type, targetField, runtimeMethodInfoField);
        }

        #endregion
    }
}
