using DialogMaker.Core.Scripting.Compiler.Ast;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of D# member
    /// </summary>
    public interface IDSharpMemberInfo
    {
        /// <summary>
        /// Assembly that contains current member
        /// </summary>
        public IDSharpAssembly Assembly { get; }
        /// <summary>
        /// Name of member
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Metadata token to identify member
        /// </summary>
        public DSharpMetadataToken MetadataToken { get; }
        /// <summary>
        /// Type that declares current member
        /// </summary>
        public IDSharpType DeclaringType { get; }
        /// <summary>
        /// Access modifier of current member
        /// </summary>
        public DSharpAccessModifier Access { get; }
        /// <summary>
        /// Is static member. Static members can be accessed without object instance
        /// </summary>
        public bool IsStatic { get; }
        /// <summary>
        /// This flag indicates that current member requiers implementation
        /// </summary>
        public bool IsDeclaration { get; }
    }
}
