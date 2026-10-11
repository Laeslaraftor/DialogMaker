namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of D# type member that can be called
    /// </summary>
    public interface IDSharpCallableMember : IDSharpMemberInfo
    {
        /// <summary>
        /// Amount of required parameters
        /// </summary>
        public int ParametersCount { get; }
        /// <summary>
        /// Amount of generic types for replacing on calling
        /// </summary>
        public int GenericParametersCount { get; }
        /// <summary>
        /// Type that return by current member
        /// </summary>
        public IDSharpType? ReturnType { get; }
        /// <summary>
        /// Callable that was overriden by current callable
        /// </summary>
        public IDSharpCallableMember? OverridenCallable { get; }
        /// <summary>
        /// Flag that indicates direct calling availability.
        /// Members that can be called directly returns only 1 method in <see cref="GetMethods"/>.
        /// For example, methods and operators can be called directly, 
        /// and indexers can not because they contains 2 methods (getter and setter)
        /// </summary>
        public bool CanBeCalledDirectly { get; }

        /// <summary>
        /// Get array of required for calling parameters
        /// </summary>
        /// <returns>Array of required for calling parameters</returns>
        public IDSharpParameterInfo[] GetParameters();
        /// <summary>
        /// Get array of generic types
        /// </summary>
        /// <returns>Array of generic types</returns>
        public IDSharpType[] GetGenericParameters();
        /// <summary>
        /// Get enumeration of methods that represents current callable.
        /// It can be empty
        /// </summary>
        /// <returns>Enumeration of methods</returns>
        public IEnumerable<IDSharpMethodInfo> GetMethods();
    }
}
