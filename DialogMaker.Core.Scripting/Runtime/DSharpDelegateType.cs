using System.Collections.ObjectModel;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about delegate type
    /// </summary>
    public class DSharpDelegateType(IDSharpType type, IDSharpMethodInfo invokeMethod, IDSharpMethodInfo? dynamicInvokeMethod, IDSharpMethodInfo instanceConstructor, IDSharpMethodInfo staticConstructor)
    {
        /// <summary>
        /// Delegate type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Invocation methods
        /// </summary>
        public IDSharpMethodInfo InvokeMethod { get; } = invokeMethod;
        /// <summary>
        /// Method for dynamic invocation
        /// </summary>
        public IDSharpMethodInfo? DynamicInvokeMethod { get; } = dynamicInvokeMethod;
        /// <summary>
        /// Constructor that creates delegate for method with object instance
        /// </summary>
        public IDSharpMethodInfo InstanceConstructor { get; } = instanceConstructor;
        /// <summary>
        /// Constructor that creates delegate for static method
        /// </summary>
        public IDSharpMethodInfo StaticConstructor { get; } = staticConstructor;

        #region Constants

        /// <summary>
        /// Name of method for invoking delegate
        /// </summary>
        public const string InvokeMethodName = "Invoke";
        /// <summary>
        /// Name of method for dynamic method invocation
        /// </summary>
        public const string DynamicInvokeMethodName = "DynamicInvoke";
        /// <summary>
        /// Field that contains address to runtime method information
        /// </summary>
        public const string RuntimeMethodInfoFieldName = "_runtimeMethodInfo";
        /// <summary>
        /// Field that contains object instance for invoking instance method
        /// </summary>
        public const string TargetFieldName = "_target";

        #endregion

        #region Static

        /// <summary>
        /// Create information about delegate type
        /// </summary>
        /// <param name="type">Delegate type for that need to create information</param>
        /// <returns>Information about provided delegate type</returns>
        /// <exception cref="ArgumentException">Delegate type was required</exception>
        public static DSharpDelegateType Create(IDSharpType type)
        {
            if (type.ObjectType != DSharpObjectType.Delegate)
            {
                throw new ArgumentException("Delegate type was required", nameof(type));
            }

            var invokeMethods = type.GetMethodOrDefault(InvokeMethodName)
                ?? throw new ArgumentException($"Type \"{type}\" not contains \"{InvokeMethodName}\" method", nameof(type));
            var dynamicInvokeMethod = type.GetMethodOrDefault(DynamicInvokeMethodName);
            IDSharpMethodInfo? instanceConstructor = null;
            IDSharpMethodInfo? staticConstructor = null;

            foreach (var constructor in type.GetConstructors())
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length == 0 ||
                    parameters[0].Type.FullName != DSharpBuildInTypes.IntPtr)
                {
                    continue;
                }
                if (parameters.Length == 1)
                {
                    staticConstructor = constructor;
                }
                else if (parameters[1].Type == type.Assembly.ObjectType)
                {
                    instanceConstructor = constructor;
                }
            }

            if (instanceConstructor == null)
            {
                throw new ArgumentException("Constructor that creates delegate for instance method not found", nameof(type));
            }
            if (staticConstructor == null)
            {
                throw new ArgumentException("Constructor that creates delegate for static method not found", nameof(type));
            }

            return new(type, new(invokeMethods), dynamicInvokeMethod, instanceConstructor, staticConstructor);
        }

        #endregion
    }
}
