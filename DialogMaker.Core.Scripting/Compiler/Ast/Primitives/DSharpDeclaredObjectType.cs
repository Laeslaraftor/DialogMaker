using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Type of declared object
    /// </summary>
    public enum DSharpDeclaredObjectType
    {
        /// <summary>
        /// Object is class
        /// </summary>
        Class = DSharpObjectType.Class,
        /// <summary>
        /// Object is struct
        /// </summary>
        Struct = DSharpObjectType.Struct,
        /// <summary>
        /// Object is interface
        /// </summary>
        Interface = DSharpObjectType.Interface
    }
}
