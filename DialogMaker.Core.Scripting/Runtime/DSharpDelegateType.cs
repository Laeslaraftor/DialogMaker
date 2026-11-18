using static DialogMaker.Core.Scripting.Runtime.DSharpDelegateType;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about D# delegate type
    /// </summary>
    public class DSharpDelegateType(IDSharpType type, Info info)
    {
        /// <summary>
        /// Delegate type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Property that contains information about method
        /// </summary>
        public IDSharpPropertyInfo MethodInfoProperty { get; } = info.MethodInfoProperty;
        /// <summary>
        /// Property that contains target invocation instance
        /// </summary>
        public IDSharpPropertyInfo TargetProperty { get; } = info.TargetProperty;
        /// <summary>
        /// Field that contains invocation list
        /// </summary>
        public IDSharpFieldInfo InvocationListField { get; } = info.InvocationListField;
        /// <summary>
        /// Method for dynamic invocation
        /// </summary>
        public IDSharpMethodInfo DynamicInvokeMethod { get; } = info.DynamicInvokeMethod;
        /// <summary>
        /// Constructor for creating delegate instance with target invocation instance
        /// </summary>
        public IDSharpMethodInfo InstanceConstructor { get; } = info.InstanceConstructor;
        /// <summary>
        /// Constructor for creating delegate instance for static method
        /// </summary>
        public IDSharpMethodInfo StaticConstructor { get; } = info.StaticConstructor;
        /// <summary>
        /// Constructor for creating delegate instance with specified invocation list length
        /// </summary>
        public IDSharpMethodInfo CapacityConstructor { get; } = info.CapacityConstructor;
        /// <summary>
        /// Method for creating delegate instance with specified invocation list length
        /// </summary>
        public IDSharpMethodInfo CreateNewMethod { get; } = info.CreateNewMethod;

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
        /// Name of field that contains invocation list
        /// </summary>
        public const string InvocationListFieldName = "InvocationList";
        /// <summary>
        /// Name of property that contains object instance for invoking instance method
        /// </summary>
        public const string TargetPropertyName = "Target";
        /// <summary>
        /// Name of method for creating new delegate instance with specified invocation list length
        /// </summary>
        public const string CreateNewMethodName = "CreateNew";

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
            return new(type, info);
        }

        /// <summary>
        /// Create information about delegate type
        /// </summary>
        /// <param name="type">Delegate type for creating information about it</param>
        /// <returns>Information about provided delegate type</returns>
        /// <exception cref="ArgumentException">Constructor that creates delegate for instance method not found</exception>
        /// <exception cref="ArgumentException">Constructor that creates delegate for static method not found</exception>
        public static Info CreateInfo(IDSharpType type)
        {
            var dynamicInvoke = type.GetMethod(DynamicInvokeMethodName);
            var methodInfoProperty = type.GetProperty(MethodInfoPropertyName);
            var targetProperty = type.GetProperty(TargetPropertyName);
            var invocationListField = type.GetField(InvocationListFieldName);
            IDSharpMethodInfo? instanceConstructor = null;
            IDSharpMethodInfo? staticConstructor = null;
            IDSharpMethodInfo? capacityConstructor = null;
            IDSharpMethodInfo? createNewMethod = type.GetMethodOrDefault(CreateNewMethodName)
                ?? throw new ArgumentException($"Method \"{CreateNewMethodName}\" at \"{type}\" not found", nameof(type));

            foreach (var constructor in type.GetConstructors())
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length == 0)
                {
                    continue;
                }
                if (parameters.Length == 1 && parameters[0].Type.FullName == DSharpBuildInTypes.IntPtr)
                {
                    staticConstructor = constructor;
                }
                else if (parameters.Length == 1 && parameters[0].Type.FullName == DSharpBuildInTypes.Int32)
                {
                    capacityConstructor = constructor;
                }
                else if (parameters.Length == 2 && parameters[0].Type == type.Assembly.ObjectType)
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
            if (capacityConstructor == null)
            {
                throw new ArgumentException("Constructor that creates delegate with specified invocation list length not found", nameof(type));
            }

            return new(dynamicInvoke, methodInfoProperty, targetProperty, invocationListField, instanceConstructor, staticConstructor, capacityConstructor, createNewMethod);
        }

        #endregion

        #region Structs

        /// <summary>
        /// Information about delegate type
        /// </summary>
        /// <param name="DynamicInvokeMethod"><inheritdoc cref="DynamicInvokeMethod"/></param>
        /// <param name="MethodInfoProperty"><inheritdoc cref="MethodInfoProperty"/></param>
        /// <param name="TargetProperty"><inheritdoc cref="TargetProperty"/></param>
        /// <param name="InvocationListField"><inheritdoc cref="InvocationListField"/></param>
        /// <param name="InstanceConstructor"><inheritdoc cref="InstanceConstructor"/></param>
        /// <param name="StaticConstructor"><inheritdoc cref="StaticConstructor"/></param>
        /// <param name="CapacityConstructor"><inheritdoc cref="CapacityConstructor"/></param>
        /// <param name="CreateNewMethod"><inheritdoc cref="CreateNewMethod"/></param>
        public record struct Info(IDSharpMethodInfo DynamicInvokeMethod, IDSharpPropertyInfo MethodInfoProperty, IDSharpPropertyInfo TargetProperty, IDSharpFieldInfo InvocationListField, IDSharpMethodInfo InstanceConstructor, IDSharpMethodInfo StaticConstructor, IDSharpMethodInfo CapacityConstructor, IDSharpMethodInfo CreateNewMethod);

        #endregion
    }
}
