namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about D# delegate type
    /// </summary>
    public class DSharpDelegateType(IDSharpType type, IDSharpPropertyInfo methodInfoProperty, IDSharpPropertyInfo targetProperty, IDSharpMethodInfo dynamicInvokeMethod, IDSharpMethodInfo? instanceConstructor, IDSharpMethodInfo? staticConstructor)
    {
        /// <summary>
        /// Delegate type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Property that contains information about method
        /// </summary>
        public IDSharpPropertyInfo MethodInfoProperty { get; } = methodInfoProperty;
        /// <summary>
        /// Property that contains target invocation instance
        /// </summary>
        public IDSharpPropertyInfo TargetProperty { get; } = targetProperty;
        /// <summary>
        /// Method for dynamic invocation
        /// </summary>
        public IDSharpMethodInfo DynamicInvokeMethod { get; } = dynamicInvokeMethod;
        /// <summary>
        /// Constructor for creating delegate instance with target invocation instance
        /// </summary>
        public IDSharpMethodInfo? InstanceConstructor { get; } = instanceConstructor;
        /// <summary>
        /// Constructor for creating delegate instance for static method
        /// </summary>
        public IDSharpMethodInfo? StaticConstructor { get; } = staticConstructor;

        #region Constants

        /// <summary>
        /// Name of method for dynamic method invocation
        /// </summary>
        public const string DynamicInvokeMethodName = "DynamicInvoke";
        /// <summary>
        /// Name of property that contains information about invoking method
        /// </summary>
        public const string MethodInfoPropertyName = "MethodInfo";
        /// <summary>
        /// Name of property that contains object instance for invoking instance method
        /// </summary>
        public const string TargetPropertyName = "Target";

        #endregion

        #region Static

        /// <summary>
        /// Create information about <see cref="DSharpBuildInTypes.Extra.Delegate"/> type that contains in specified assembly
        /// </summary>
        /// <param name="assembly">Assembly that contains delegate type</param>
        /// <returns>Information about delegate type that was found</returns>
        public static DSharpDelegateType Create(IDSharpAssembly assembly)
        {
            return Create(assembly.GetType(DSharpBuildInTypes.Extra.Delegate));
        }
        /// <summary>
        /// Create information about delegate type
        /// </summary>
        /// <param name="type">Delegate type for creating information about it</param>
        /// <returns>Information about provided delegate type</returns>
        /// <exception cref="ArgumentException">Constructor that creates delegate for instance method not found</exception>
        /// <exception cref="ArgumentException">Constructor that creates delegate for static method not found</exception>
        public static DSharpDelegateType Create(IDSharpType type)
        {
            var info = CreateInfo(type);
            return new(type, info.MethodInfoProperty, info.TargetProperty, info.DynamicInvokeMethod, info.InstanceConstructor, info.StaticConstructor);
        }

        /// <summary>
        /// Create information about delegate type
        /// </summary>
        /// <param name="type">Delegate type for creating information about it</param>
        /// <returns>Information about provided delegate type</returns>
        /// <exception cref="ArgumentException">Constructor that creates delegate for instance method not found</exception>
        /// <exception cref="ArgumentException">Constructor that creates delegate for static method not found</exception>
        protected static Info CreateInfo(IDSharpType type)
        {
            var dynamicInvoke = type.GetMethod(DynamicInvokeMethodName);
            var methodInfoProperty = type.GetProperty(MethodInfoPropertyName);
            var targetProperty = type.GetProperty(TargetPropertyName);
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

            return new(dynamicInvoke, methodInfoProperty, targetProperty, instanceConstructor, staticConstructor);
        }

        #endregion

        #region Structs

        /// <summary>
        /// Information about delegate type
        /// </summary>
        /// <param name="DynamicInvokeMethod"><inheritdoc cref="DynamicInvokeMethod"/></param>
        /// <param name="MethodInfoProperty"><inheritdoc cref="MethodInfoProperty"/></param>
        /// <param name="TargetProperty"><inheritdoc cref="TargetProperty"/></param>
        /// <param name="InstanceConstructor"><inheritdoc cref="InstanceConstructor"/></param>
        /// <param name="StaticConstructor"><inheritdoc cref="StaticConstructor"/></param>
        protected record struct Info(IDSharpMethodInfo DynamicInvokeMethod, IDSharpPropertyInfo MethodInfoProperty, IDSharpPropertyInfo TargetProperty, IDSharpMethodInfo? InstanceConstructor, IDSharpMethodInfo? StaticConstructor);

        #endregion
    }
}
