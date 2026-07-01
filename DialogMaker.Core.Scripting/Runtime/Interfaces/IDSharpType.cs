using DialogMaker.Core.Scripting.Compiler.Ast;

namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpType : IDSharpMemberInfo
    {
        /// <summary>
        /// Is type generic. 
        /// Generic types is template for normal types
        /// </summary>
        public bool IsGeneric { get; }
        /// <summary>
        /// Namespace of this type
        /// </summary>
        public string? Namespace { get; }
        /// <summary>
        /// Type full name. 
        /// It contains namespace, name and generic parameters or types amount
        /// </summary>
        public string FullName { get; }
        /// <summary>
        /// Type of object
        /// </summary>
        public DSharpObjectType ObjectType { get; }
        /// <summary>
        /// Is type abstract. 
        /// Abstract types can not be instantiated, but it's can contains abstract members
        /// </summary>
        public bool IsAbstract { get; }
        /// <summary>
        /// Is type sealed. Sealed types can not be inherited
        /// </summary>
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
        /// <summary>
        /// Size of type in bytes
        /// </summary>
        public int Size { get; }

        public IDSharpType[] GetBaseTypes();
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

        /// <summary>
        /// Get all constructors that passes specified predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>All constructors that passes specified predicate</returns>
        public IDSharpMethodInfo[] GetConstructors(Predicate<IDSharpMethodInfo> predicate);
        /// <summary>
        /// Get all methods that passes specified predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>All methods that passes specified predicate</returns>
        public IDSharpMethodInfo[] GetMethods(Predicate<IDSharpMethodInfo> predicate);
        /// <summary>
        /// Get all properties that passes specified predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>All properties that passes specified predicate</returns>
        public IDSharpPropertyInfo[] GetProperties(Predicate<IDSharpPropertyInfo> predicate);
        /// <summary>
        /// Get all fields that passes specified predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>All fields that passes specified predicate</returns>
        public IDSharpFieldInfo[] GetFields(Predicate<IDSharpFieldInfo> predicate);
        /// <summary>
        /// Get all indexers that passes specified predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>All indexers that passes specified predicate</returns>

        public IDSharpIndexerInfo[] GetIndexers(Predicate<IDSharpIndexerInfo> predicate);

        /// <summary>
        /// Get all custom cast operators for this type
        /// </summary>
        /// <returns>Array of custom cast operators</returns>
        public IDSharpOperatorInfo[] GetCastOperators();
        /// <summary>
        /// Get all custom math operators for this type
        /// </summary>
        /// <returns>Array of custom math operators</returns>
        public IDSharpOperatorInfo[] GetOperators();
    }
}
