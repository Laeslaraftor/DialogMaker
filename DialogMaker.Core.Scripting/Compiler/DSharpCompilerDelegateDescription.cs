using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Description of compiled delegate
    /// </summary>
    /// <param name="type">Delegate type</param>
    public class DSharpCompilerDelegateDescription(DSharpTypeBuilder type, DSharpMethodBuilder instanceConstructor, DSharpMethodBuilder staticConstructor, DSharpMethodBuilder capacityConstructor, DSharpMethodBuilder createNewMethod)
    {
        /// <summary>
        /// Delegate type
        /// </summary>
        public DSharpTypeBuilder Type { get; } = type;
        /// <summary>
        /// Delegate definition and invoke methods that represents it
        /// </summary>
        public Dictionary<DSharpMethodBuilder, ObjectDeclarationNode> InvokeMethods { get; } = [];
        /// <summary>
        /// Constructor that creates delegate for method with object instance
        /// </summary>
        public DSharpMethodBuilder InstanceConstructor { get; } = instanceConstructor;
        /// <summary>
        /// Constructor that creates delegate for static method
        /// </summary>
        public DSharpMethodBuilder StaticConstructor { get; } = staticConstructor;
        /// <summary>
        /// Constructor that creates delegate with specified invocation list length
        /// </summary>
        public DSharpMethodBuilder CapacityConstructor { get; } = capacityConstructor;
        /// <summary>
        /// Method that creates delegate with specified invocation list length
        /// </summary>
        public DSharpMethodBuilder CreateNewMethod { get; } = createNewMethod;
    }
}
