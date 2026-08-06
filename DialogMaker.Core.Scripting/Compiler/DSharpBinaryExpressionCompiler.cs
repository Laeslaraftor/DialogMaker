using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// D# binary expression compiler
    /// </summary>
    public static class DSharpBinaryExpressionCompiler
    {
        /// <summary>
        /// Compile binary expression to pair (left + operator + right)
        /// </summary>
        /// <param name="expression">Binary expression for compiling</param>
        /// <returns>Binary expression pair for compiling to get result</returns>
        /// <exception cref="InvalidOperationException">Left side pairs can not be null when it compiled as binary</exception>
        /// <exception cref="InvalidOperationException">Right side pairs can not be null when it compiled as binary</exception>
        public static IEnumerable<BinaryPair> Compile(BinaryExpressionNode expression)
        {
            var leftPairs = RecursiveCompile(expression.Left, out bool leftCompiledAsBinary, out var leftExpression);
            var rightPairs = RecursiveCompile(expression.Right, out bool rightCompiledAsBinary, out var rightExpression);

            if (leftCompiledAsBinary)
            {
                if (leftPairs == null)
                {
                    throw new InvalidOperationException($"Left side pairs can not be null when it compiled as binary: {leftExpression}");
                }

                foreach (var leftPair in leftPairs)
                {
                    yield return leftPair;
                }
            }

            if (rightCompiledAsBinary && !leftCompiledAsBinary)
            {
                yield return new(BinaryPairCompileType.CompileLeftBeforeRight, leftExpression, null, expression.Operator);
            }

            if (rightCompiledAsBinary)
            {
                if (rightPairs == null)
                {
                    throw new InvalidOperationException($"Right side pairs can not be null when it compiled as binary: {rightExpression}");
                }

                foreach (var rightPair in rightPairs)
                {
                    yield return rightPair;
                }
            }

            if (rightCompiledAsBinary && !leftCompiledAsBinary)
            {
                yield return new(BinaryPairCompileType.CompileLeftAfterRight, leftExpression, null, expression.Operator);
                yield break;
            }

            if (leftCompiledAsBinary && !rightCompiledAsBinary)
            {
                yield return new(BinaryPairCompileType.PreviousAsLeft, null, rightExpression, expression.Operator);
                yield break;
            }
            else if (leftCompiledAsBinary && rightCompiledAsBinary)
            {
                yield return new(BinaryPairCompileType.WithPrevious, null, null, expression.Operator);
                yield break;
            }

            yield return new(BinaryPairCompileType.Default, leftExpression, rightExpression, expression.Operator);
        }

        private static IEnumerable<BinaryPair>? RecursiveCompile(ExpressionNode? expression, out bool compiledAsBinary, out ExpressionNode targetExpression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            while (true)
            {
                if (expression is BinaryExpressionNode binaryExpression)
                {
                    compiledAsBinary = true;
                    targetExpression = binaryExpression;

                    return Compile(binaryExpression);
                }
                else if (expression is ParenContainedExpressionNode parenContainedExpression)
                {
                    expression = parenContainedExpression.Expression;
                }
                else
                {
                    break;
                }
            }

            if (expression == null)
            {
                throw new InvalidOperationException($"Target expression can not be null: {expression}");
            }

            compiledAsBinary = false;
            targetExpression = expression;

            return null;
        }

        public readonly struct BinaryPair(BinaryPairCompileType type, ExpressionNode? left, ExpressionNode? right, DSharpBinaryOperator @operator)
        {
            /// <summary>
            /// Type of pair compiling
            /// </summary>
            public BinaryPairCompileType Type { get; } = type;
            /// <summary>
            /// Left side of binary expression.
            /// If this side not provided then use result from previous pair as left side
            /// </summary>
            public ExpressionNode? Left { get; } = left;
            /// <summary>
            /// Right side of binary expression.
            /// If this side not provided then use result from previous pair as right side
            /// </summary>
            public ExpressionNode? Right { get; } = right;
            /// <summary>
            /// Binary binary operation between two expression
            /// </summary>
            public DSharpBinaryOperator Operator { get; } = @operator;

            public override string ToString()
            {
                StringBuilder builder = new();
                builder.AppendLine($"{Type}:{Operator}");

                if (Left != null)
                {
                    builder.AppendLine($"Left: {Left}");
                }
                if (Right != null)
                {
                    builder.AppendLine($"Right: {Right}");
                }

                return builder.ToString().TrimEnd();
            }
        }
        /// <summary>
        /// Binary pair compiling type
        /// </summary>
        public enum BinaryPairCompileType
        {
            /// <summary>
            /// Default compiling binary operation between two expression
            /// </summary>
            Default,
            /// <summary>
            /// Left expression was not provided.
            /// This compiling performs with value that was got from previous binary operation (left side).
            /// Right side provided normally
            /// </summary>
            PreviousAsLeft,
            /// <summary>
            /// Left expression was provided, but right not.
            /// Compile left expression before complex right expression.
            /// </summary>
            CompileLeftBeforeRight,
            /// <summary>
            /// Left expression was provided, but right not.
            /// This is continue of <see cref="CompileLeftBeforeRight"/>, now right expression compiled.
            /// This type purposed for setting up binary operator
            /// </summary>
            CompileLeftAfterRight,
            /// <summary>
            /// Both expression sides not provided.
            /// This compiling performs with values that was got from previous binary operations (left and right side)
            /// </summary>
            WithPrevious,
        }
    }
}
