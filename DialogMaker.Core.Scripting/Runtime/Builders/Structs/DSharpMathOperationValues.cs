using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    internal readonly struct DSharpMathOperationValues(ExpressionNode left, ExpressionNode right) 
        : IEquatable<DSharpMathOperationValues>
    {
        public ExpressionNode Left { get; } = left;
        public ExpressionNode Right { get; } = right;

        #region Управление

        public bool Contains(ExpressionNode expression)
        {
            return Left == expression || Right == expression;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Right);
        }
        public override bool Equals(object obj)
        {
            return obj is DSharpMathOperationValues other && Equals(other);
        }
        public bool Equals(DSharpMathOperationValues other)
        {
            return Left == other.Left &&
                   Right == other.Right;
        }

        #endregion
    }
}
