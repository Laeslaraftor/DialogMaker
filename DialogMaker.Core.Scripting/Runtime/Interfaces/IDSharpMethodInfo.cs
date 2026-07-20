using DialogMaker.Core.Scripting.Runtime.Executor;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of D# method
    /// </summary>
    public interface IDSharpMethodInfo : IDSharpMemberInfo
    {
        /// <summary>
        /// Type of object that returns by method
        /// </summary>
        public IDSharpType? ReturnType { get; }
        /// <summary>
        /// Type of method
        /// </summary>
        public DSharpMethodType MethodType { get; }
        /// <summary>
        /// Method that was overriden by current method
        /// </summary>
        public IDSharpMethodInfo? OverrideMethod { get; }
        /// <summary>
        /// Is method virtual. 
        /// Virtual methods can be overriden and have a implementation
        /// </summary>
        public bool IsVirtual { get; }
        /// <summary>
        /// Is method abstract. Abstract methods contains only in abstract classed,
        /// they can not have implementation
        /// </summary>
        public bool IsAbstract { get; }
        /// <summary>
        /// Is method sealed. Sealed methods can not be overriden
        /// </summary>
        public bool IsSealed { get; }
        /// <summary>
        /// Is method external. External methods requires provided handler at runtime by <see cref="IDSharpExternalMethodsProvider"/>
        /// </summary>
        public bool IsExtern { get; }
        /// <summary>
        /// Method bytecode
        /// </summary>
        public IDSharpMethodBytecode? Bytecode { get; }

        /// <summary>
        /// Get array of parameters for invoke current method
        /// </summary>
        /// <returns>Array of parameters for invoke current method</returns>
        public IDSharpParameterInfo[] GetParameters();
        /// <summary>
        /// Get array of generic parameter that should be specified on invocation
        /// </summary>
        /// <returns>Array of generic parameter that should be specified on invocation</returns>
        public IDSharpType[] GetGenericParameters();
        /// <summary>
        /// Get array of interfaces method declarations that implemented by current method
        /// </summary>
        /// <returns>Array of interfaces method declarations that implemented by current method</returns>
        public IDSharpMethodInfo[] GetImplementedMethods();
    }
}
