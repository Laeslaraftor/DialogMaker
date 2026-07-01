using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents custom operator
    /// </summary>
    /// <param name="token">Token that represents operator</param>
    public class OperatorNode(DSharpToken token) : InvokableNode(token)
    {
        /// <summary>
        /// Binary operator that implement this operator
        /// </summary>
        public DSharpBinaryOperator? BinaryOperator { get; set; }
        /// <summary>
        /// Unary operator that implement this operator
        /// </summary>
        public DSharpUnaryOperator? UnaryOperator { get; set; }
        /// <summary>
        /// Type of this operator
        /// </summary>
        public DSharpOperatorType OperatorType { get; set; }
        /// <summary>
        /// Type of object that returns by this operator
        /// </summary>
        public TypeInfoNode? ReturnType { get; set; }
        /// <summary>
        /// Access modifier of this operator
        /// </summary>
        public DSharpAccessModifier Access { get; set; }

        #region Статика

        /// <summary>
        /// Parse operator node starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <param name="memberInfo">Information about operator</param>
        /// <returns>Parsed operator node</returns>
        public static OperatorNode Parse(AstParserStream stream, ObjectDeclarationNode.MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != DSharpTypeMember.Operator)
            {
                throw new ArgumentException($"Required information for operator but provided for {memberInfo.MemberType}", nameof(memberInfo));
            }
            if (!memberInfo.IsStatic)
            {
                stream.ThrowPositionException($"Operator should be static");
            }

            OperatorNode result;

            if (memberInfo.OperatorType == DSharpOperatorType.Implicit ||
                memberInfo.OperatorType == DSharpOperatorType.Explicit)
            {
                result = new(stream.Peek(-2)!)
                {
                    ReturnType = TypeInfoNode.Parse(stream, true, true)
                };
                result.Identifier = new(result.ReturnType.Token);
            }
            else
            {
                if (BinaryExpressionNode.IsBinaryOperator(stream))
                {
                    memberInfo.OperatorType = DSharpOperatorType.Binary;
                    var token = stream.Eat(stream.Current!.Type);
                    result = new(token)
                    {
                        Identifier = new(token),
                        BinaryOperator = (DSharpBinaryOperator)token.Type
                    };
                }
                else if (UnaryExpressionNode.IsUnaryOperator(stream))
                {
                    memberInfo.OperatorType = DSharpOperatorType.Unary;
                    var token = stream.Eat(stream.Current!.Type);
                    result = new(token)
                    {
                        Identifier = new(token),
                        UnaryOperator = (DSharpUnaryOperator)token.Type
                    };
                }
                else
                {
                    stream.ThrowPositionException("Invalid operator");
                    return null;
                }

                result.ReturnType = memberInfo.Type;
            }

            ParseParameters(stream, result.Parameters);

            if ((memberInfo.OperatorType == DSharpOperatorType.Implicit ||
                memberInfo.OperatorType == DSharpOperatorType.Explicit ||
                memberInfo.OperatorType == DSharpOperatorType.Unary) &&
                result.Parameters.Count != 1)
            {
                stream.ThrowPositionException($"Implicit, explicit and unary operator should contains only 1 parameter, got {result.Parameters.Count}");
            }
            if (memberInfo.OperatorType == DSharpOperatorType.Binary &&
                result.Parameters.Count != 2)
            {
                stream.ThrowPositionException($"Binary operator should contains 2 parameters, got {result.Parameters.Count}");
            }

            if (stream.Check(DSharpTokenType.LeftBrace))
            {
                result.Body = BlockStatementNode.Parse(stream);
            }
            else if (stream.Check(DSharpTokenType.Lambda))
            {
                result.Body = BlockStatementNode.Parse(stream, DSharpTokenType.Semicolon, DSharpTokenType.Lambda);
            }
            else
            {
                stream.ThrowPositionException("Operator must have a body");
            }

            result.OperatorType = memberInfo.OperatorType;
            result.Attributes = memberInfo.Attributes;
            result.Access = memberInfo.AccessModifier;

            return result;
        }

        #endregion
    }
}
