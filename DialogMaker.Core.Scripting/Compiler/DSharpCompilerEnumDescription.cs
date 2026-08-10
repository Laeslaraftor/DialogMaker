using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Compiler description of enum type
    /// </summary>
    public class DSharpCompilerEnumDescription(DSharpTypeBuilder typeBuilder, ObjectDeclarationNode declarationNode)
    {
        /// <summary>
        /// Enum type builder
        /// </summary>
        public DSharpTypeBuilder TypeBuilder { get; } = typeBuilder;
        /// <summary>
        /// Enum declaration
        /// </summary>
        public ObjectDeclarationNode DeclarationNode { get; } = declarationNode;
        /// <summary>
        /// Enum fields with it's declarations
        /// </summary>
        public Dictionary<DSharpFieldBuilder, FieldNode>? ValueFields { get; set; }
        /// <summary>
        /// Method that override <c>ToString</c> and return name of current value
        /// </summary>
        public DSharpMethodBuilder? ToStringMethod { get; set; }
        /// <summary>
        /// Constructor for initializing enum value
        /// </summary>
        public DSharpMethodBuilder? ValueConstructor { get; set; }
        /// <summary>
        /// Instance field that contain enum value
        /// </summary>
        public DSharpFieldBuilder? InstanceValueField { get; set; }
        /// <summary>
        /// Explicit operator for converting enum to his value
        /// </summary>
        public DSharpOperatorBuilder? ExplicitEnumToValueOperator { get; set; }
        /// <summary>
        /// Explicit operator for converting value to enum
        /// </summary>
        public DSharpOperatorBuilder? ExplicitValueToEnumOperator { get; set; }
    }
}
