using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Base class of all statements
    /// </summary>
    /// <param name="token">Token that represents some statement</param>
    public abstract class StatementNode(DialogScriptToken token) : AstNode(token)
    {
        public DialogScriptToken Token { get; } = token;

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"{GetType().Name}({Token.Value}) at {base.ToString()}";
        }

        #endregion

        #region Статика

        /// <summary>
        /// Parse some statement
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed statement</returns>
        /// <exception cref="Exception"></exception>
        public static StatementNode ParseStatement(AstParserStream stream)
        {
            if (stream.Check(DialogScriptTokenType.LeftBrace))
            {
                return BlockStatementNode.Parse(stream);
            }

            AttributeNode.TryParse(stream, out var attributes);

            InvokableStatementNode ParseInvokable(Func<AstParserStream, StructNode.MemberInfo, InvokableNode> parser, StructNode.MemberInfo memberInfo)
            {
                var method = parser(stream, memberInfo);
                method.Attributes = attributes;

                return new InvokableStatementNode(method.Token)
                {
                    Invokable = method
                };
            }
            StatementNode ParseStructMember(StructNode.MemberInfo memberInfo)
            {
                if (memberInfo.MemberType == DialogScriptTypeMember.Method)
                {
                    return ParseInvokable(MethodNode.Parse, memberInfo);
                }
                else if (memberInfo.MemberType == DialogScriptTypeMember.Constructor)
                {
                    return ParseInvokable(ConstructorNode.Parse, memberInfo);
                }
                else if (memberInfo.MemberType == DialogScriptTypeMember.Field)
                {
                    var field = FieldNode.ParseField(stream, memberInfo);
                    field.Attributes = attributes;

                    return new VariableStatementNode(field.Token)
                    {
                        Variable = field
                    };
                }

                throw new Exception($"Invalid member: {memberInfo.MemberType}");
            }
            StructStatementNode ParseStruct()
            {
                var structNode = StructNode.Parse(stream);
                structNode.Attributes = attributes;

                return new StructStatementNode(structNode.Token)
                {
                    Struct = structNode
                };
            }

            if (stream.Check(DialogScriptTokenType.Var))
            {
                var variable = VariableNode.ParseVariable(stream, attributes);
                variable.Attributes = attributes;

                return new VariableStatementNode(variable.Token)
                {
                    Variable = variable
                };
            }
            if (stream.Check(DialogScriptTokenType.Extern) || stream.Check(DialogScriptTokenType.Func))
            {
                if (!StructNode.TryStartParseMember(stream, out var memberInfo))
                {
                    stream.ThrowPositionException("Invalid tokens");
                }

                return ParseInvokable(MethodNode.Parse, memberInfo);
            }
            if (StructNode.IsAccessModifier(stream))
            {
                if (StructNode.IsStructDeclaration(stream))
                {
                    ParseStruct();
                }

                if (!StructNode.TryStartParseMember(stream, out var memberInfo))
                {
                    stream.ThrowPositionException("Invalid tokens");
                }

                ParseStructMember(memberInfo);
            }
            if (stream.Check(DialogScriptTokenType.Struct))
            {
                return ParseStruct();
            }
            if (stream.Check(DialogScriptTokenType.Enum))
            {
                var enumNode = EnumNode.Parse(stream);

                return new EnumStatementNode(enumNode.Token)
                {
                    Enum = enumNode
                };
            }
            if (stream.Check(DialogScriptTokenType.If))
            {
                return IfStatementNode.Parse(stream);
            }
            if (stream.Check(DialogScriptTokenType.While))
            {
                return WhileStatementNode.Parse(stream);
            }
            if (stream.Check(DialogScriptTokenType.For))
            {
                return ForStatementNode.Parse(stream);
            }
            if (stream.Check(DialogScriptTokenType.Return))
            {
                return ReturnStatementNode.Parse(stream);
            }
            if (stream.Check(DialogScriptTokenType.Break))
            {
                return BreakStatementNode.Parse(stream);
            }
            if (stream.Check(DialogScriptTokenType.Continue))
            {
                return ContinueStatementNode.Parse(stream);
            }
            else if (!(stream.Check(DialogScriptTokenType.Identifier) && stream.Check(DialogScriptTokenType.LeftParen, 1)) &&
                     StructNode.TryStartParseMember(stream, out var structMemberInfo))
            {
                return ParseStructMember(structMemberInfo);
            }

            var expression = ExpressionNode.ParseExpression(stream);

            if (!stream.Check(DialogScriptTokenType.RightBrace))
            {
                stream.Eat(DialogScriptTokenType.Semicolon);
            }

            return new ExpressionStatementNode(expression.Token)
            { 
                Expression = expression 
            };
        }

        #endregion
    }
}
