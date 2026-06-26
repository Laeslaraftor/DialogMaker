namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpType : IDSharpMemberInfo
    {
        public bool IsGeneric { get; }
        public string? Namespace { get; }
        public string FullName { get; }
        public DSharpObjectType ObjectType { get; }
        public bool IsAbstract { get; }
        public bool IsSealed { get; }
        /// <summary>
        /// Finalizer/destructor of this object.
        /// This property provides finalizer only for current type,
        /// if is this property empty it's not indicates that finalizer 
        /// will not called at all. 
        /// Finalizer always virtual and it may be contained by base type.
        /// Anyway this method always have same signature: <c>protected virtual void Finalize()</c>
        /// </summary>
        public IDSharpMethodInfo? Finalizer { get; }
        /// <summary>
        /// Type that used as template to create current type
        /// </summary>
        public IDSharpType? GenericTemplate { get; }

        public IDSharpType[] GetBaseTypes();
        public IDSharpMethodInfo[] GetConstructors();
        public IDSharpMethodInfo[] GetConstructors(Predicate<IDSharpMethodInfo> predicate);
        public IDSharpMethodInfo[] GetMethods();
        public IDSharpMethodInfo[] GetMethods(Predicate<IDSharpMethodInfo> predicate);
        public IDSharpPropertyInfo[] GetProperties();
        public IDSharpPropertyInfo[] GetProperties(Predicate<IDSharpPropertyInfo> predicate);
        public IDSharpFieldInfo[] GetFields();
        public IDSharpFieldInfo[] GetFields(Predicate<IDSharpFieldInfo> predicate);
        public IDSharpIndexerInfo[] GetIndexers();
        public IDSharpIndexerInfo[] GetIndexers(Predicate<IDSharpIndexerInfo> predicate);
        /// <summary>
        /// List of types that must fill generic types. 
        /// Size of this list must be equals to generic types list or empty
        /// </summary>
        public IDSharpType[] GetGenericParameters();
        /// <summary>
        /// Generic types that created by this type
        /// </summary>
        public IDSharpType[] GetGenericTypes();
        /// <summary>
        /// Get all types that contains in current type
        /// </summary>
        /// <returns>Array of types that contain in current type</returns>
        public IDSharpType[] GetChildrenTypes();
    }
}
