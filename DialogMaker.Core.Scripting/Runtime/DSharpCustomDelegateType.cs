namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about delegate type
    /// </summary>
    public class DSharpCustomDelegateType(IDSharpType type, IDSharpPropertyInfo methodInfoProperty, IDSharpPropertyInfo targetProperty, IDSharpMethodInfo invokeMethod, IDSharpMethodInfo dynamicInvokeMethod, IDSharpMethodInfo instanceConstructor, IDSharpMethodInfo staticConstructor)
        : DSharpDelegateType(type, methodInfoProperty, targetProperty, dynamicInvokeMethod, instanceConstructor, staticConstructor)
    {
        /// <summary>
        /// Invocation methods
        /// </summary>
        public IDSharpMethodInfo InvokeMethod { get; } = invokeMethod;

        #region Constants

        /// <summary>
        /// Name of method for invoking delegate
        /// </summary>
        public const string InvokeMethodName = "Invoke";
        /// <summary>
        /// Field that contains address to runtime method information
        /// </summary>
        public const string RuntimeMethodInfoFieldName = "RuntimeMethodInfo";
        /// <summary>
        /// Field that contains object instance for invoking instance method
        /// </summary>
        public const string TargetFieldName = "TargetInstance";

        #endregion

        #region Static

        /// <summary>
        /// Create information about delegate type
        /// </summary>
        /// <param name="type">Delegate type for that need to create information</param>
        /// <returns>Information about provided delegate type</returns>
        /// <exception cref="ArgumentException">Delegate type was required</exception>
        public static new DSharpCustomDelegateType Create(IDSharpType type)
        {
            if (type.ObjectType != DSharpObjectType.Delegate)
            {
                throw new ArgumentException($"Delegate type was required, got \"{type.ObjectType}\" ({type})", nameof(type));
            }

            var invokeMethod = type.GetMethodOrDefault(InvokeMethodName)
                ?? throw new ArgumentException($"Type \"{type}\" not contains \"{InvokeMethodName}\" method", nameof(type));
            var info = CreateInfo(type);

            if (info.InstanceConstructor == null)
            {
                throw new ArgumentException("Constructor that creates delegate for instance method not found", nameof(type));
            }
            if (info.StaticConstructor == null)
            {
                throw new ArgumentException("Constructor that creates delegate for static method not found", nameof(type));
            }

            return new(type, info.MethodInfoProperty, info.TargetProperty, invokeMethod, info.DynamicInvokeMethod, info.InstanceConstructor, info.StaticConstructor);
        }

        #endregion
    }
}
