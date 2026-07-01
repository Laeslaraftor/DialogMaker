using DialogMaker.Core.Scripting.Compiler.Ast;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of operator
    /// </summary>
    public interface IDSharpOperatorInfo : IDSharpMemberInfo
    {
        /// <summary>
        /// Type of operator
        /// </summary>
        public DSharpOperatorType Type { get; }
        /// <summary>
        /// Binary operator that implements by this operator
        /// </summary>
        public DSharpBinaryOperator? BinaryOperator { get; }
        /// <summary>
        /// Unary operator that implements by this operator
        /// </summary>
        public DSharpUnaryOperator? UnaryOperator { get; }
        /// <summary>
        /// Type of object that returns by operator
        /// </summary>
        public IDSharpType ReturnType { get; }
        /// <summary>
        /// Operator method
        /// </summary>
        public IDSharpMethodInfo Method { get; }

        /// <summary>
        /// Get parameters of this operator
        /// </summary>
        /// <returns>Array of parameters</returns>
        public IDSharpParameterInfo[] GetParameters();
    }
}
