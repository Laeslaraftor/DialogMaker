using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Base class of all statements
    /// </summary>
    /// <param name="token">Token that represents some statement</param>
    public abstract class StatementNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Parse declaration statement
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed statement</returns>
        public static StatementNode ParseDeclaration(AstParserStream stream)
        {
            var declaration = ParseDeclarationOrDefault(stream);

            if (declaration == null)
            {
                stream.ThrowUnexpectedTokenException();
            }

            return declaration;
        }
        private static StatementNode? ParseDeclarationOrDefault(AstParserStream stream)
        {
            AttributeNode.TryParse(stream, out var attributes);

            StatementNode ParseObjectMember(ObjectDeclarationNode.MemberInfo memberInfo)
            {
                var member = ObjectDeclarationNode.ParseMember(stream, memberInfo, attributes);

                if (member is InvokableNode invokable)
                {
                    return new InvokableStatementNode(invokable.Token)
                    {
                        Invokable = invokable
                    };
                }
                else if (member is FieldNode field)
                {
                    return new VariableStatementNode(field.Token)
                    {
                        Variable = field
                    };
                }

                throw new Exception($"Invalid member: {memberInfo.MemberType}");
            }
            ObjectDeclarationStatementNode ParseObjectDeclaration()
            {
                var objectDeclarationNode = ObjectDeclarationNode.Parse(stream);
                objectDeclarationNode.Attributes = attributes;

                return new ObjectDeclarationStatementNode(objectDeclarationNode.Token)
                {
                    ObjectDeclaration = objectDeclarationNode
                };
            }

            if (stream.Check(DSharpTokenType.Using))
            {
                return UsingStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Namespace))
            {
                return NamespaceStatementNode.Parse(stream);
            }
            if (ObjectDeclarationNode.IsAccessModifier(stream))
            {
                if (ObjectDeclarationNode.IsObjectDeclaration(stream))
                {
                    return ParseObjectDeclaration();
                }
                if (!ObjectDeclarationNode.TryStartParseMember(stream, out var memberInfo))
                {
                    stream.ThrowPositionException("Invalid tokens");
                }

                return ParseObjectMember(memberInfo);
            }
            if (ObjectDeclarationNode.IsObjectDeclaration(stream))
            {
                return ParseObjectDeclaration();
            }
            if (stream.Check(DSharpTokenType.Enum))
            {
                var enumNode = EnumNode.Parse(stream);

                return new EnumStatementNode(enumNode.Token)
                {
                    Enum = enumNode
                };
            }
            if (!(stream.Check(DSharpTokenType.Identifier) && stream.Check(DSharpTokenType.LeftParen, 1)) &&
                ObjectDeclarationNode.TryStartParseMember(stream, out var structMemberInfo))
            {
                return ParseObjectMember(structMemberInfo);
            }

            return null;
        }
        /// <summary>
        /// Parse code statement
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed statement</returns>
        public static StatementNode ParseCode(AstParserStream stream)
        {
            if (stream.Check(DSharpTokenType.LeftBrace))
            {
                return BlockStatementNode.Parse(stream, DSharpStatementType.Code);
            }

            if (VariableNode.IsVariable(stream))
            {
                var variable = VariableNode.ParseVariable(stream, null);

                return new VariableStatementNode(variable.Token)
                {
                    Variable = variable
                };
            }
            if (stream.Check(DSharpTokenType.Using))
            {
                return UsingVariableStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Void))
            {
                if (!ObjectDeclarationNode.TryStartParseMember(stream, out var memberInfo))
                {
                    stream.ThrowPositionException("Invalid tokens");
                }

                var method = MethodNode.Parse(stream, memberInfo);

                return new InvokableStatementNode(method.Token)
                {
                    Invokable = method
                };
            }
            if (stream.Check(DSharpTokenType.If))
            {
                return IfStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.While))
            {
                return WhileStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.For))
            {
                return ForStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Foreach))
            {
                return ForeachStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Try))
            {
                return TryStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Return))
            {
                return ReturnStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Break))
            {
                return BreakStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Continue))
            {
                return ContinueStatementNode.Parse(stream);
            }

            var expression = ExpressionNode.ParseExpression(stream);

            if (!stream.Check(DSharpTokenType.RightBrace))
            {
                stream.Eat(DSharpTokenType.Semicolon);
            }

            return new ExpressionStatementNode(expression.Token)
            {
                Expression = expression
            };
        }
        /// <summary>
        /// Parse any statement
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed statement</returns>
        public static StatementNode ParseAny(AstParserStream stream)
        {
            return ParseDeclarationOrDefault(stream) ?? ParseCode(stream);
        }

        /// <summary>
        /// Parse statement with specified type
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="type">Type of statement that need to be parsed</param>
        /// <returns>Parsed statement</returns>
        /// <exception cref="Exception"></exception>
        public static StatementNode ParseStatement(AstParserStream stream, DSharpStatementType type)
        {
            if (type == DSharpStatementType.Declaration)
            {
                return ParseDeclaration(stream);
            }
            else if (type == DSharpStatementType.Code)
            {
                return ParseCode(stream);
            }

            return ParseAny(stream);

            if (stream.Check(DSharpTokenType.LeftBrace))
            {
                return BlockStatementNode.Parse(stream, type);
            }

            AttributeNode.TryParse(stream, out var attributes);

            StatementNode ParseObjectMember(ObjectDeclarationNode.MemberInfo memberInfo)
            {
                var member = ObjectDeclarationNode.ParseMember(stream, memberInfo, attributes);

                if (member is InvokableNode invokable)
                {
                    return new InvokableStatementNode(invokable.Token)
                    {
                        Invokable = invokable
                    };
                }
                else if (member is FieldNode field)
                {
                    return new VariableStatementNode(field.Token)
                    {
                        Variable = field
                    };
                }

                throw new Exception($"Invalid member: {memberInfo.MemberType}");
            }
            ObjectDeclarationStatementNode ParseObjectDeclaration()
            {
                var objectDeclarationNode = ObjectDeclarationNode.Parse(stream);
                objectDeclarationNode.Attributes = attributes;

                return new ObjectDeclarationStatementNode(objectDeclarationNode.Token)
                {
                    ObjectDeclaration = objectDeclarationNode
                };
            }

            if (stream.Check(DSharpTokenType.Using))
            {
                return UsingStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Namespace))
            {
                return NamespaceStatementNode.Parse(stream);
            }
            if (VariableNode.IsVariable(stream))
            {
                var variable = VariableNode.ParseVariable(stream, attributes);
                variable.Attributes = attributes;

                return new VariableStatementNode(variable.Token)
                {
                    Variable = variable
                };
            }
            if (stream.Check(DSharpTokenType.Extern) || stream.Check(DSharpTokenType.Void))
            {
                if (!ObjectDeclarationNode.TryStartParseMember(stream, out var memberInfo))
                {
                    stream.ThrowPositionException("Invalid tokens");
                }

                var method = MethodNode.Parse(stream, memberInfo);

                return new InvokableStatementNode(method.Token)
                {
                    Invokable = method
                };
            }
            if (ObjectDeclarationNode.IsAccessModifier(stream))
            {
                if (ObjectDeclarationNode.IsObjectDeclaration(stream))
                {
                    return ParseObjectDeclaration();
                }
                if (!ObjectDeclarationNode.TryStartParseMember(stream, out var memberInfo))
                {
                    stream.ThrowPositionException("Invalid tokens");
                }

                return ParseObjectMember(memberInfo);
            }
            if (ObjectDeclarationNode.IsObjectDeclaration(stream))
            {
                return ParseObjectDeclaration();
            }
            if (stream.Check(DSharpTokenType.Enum))
            {
                var enumNode = EnumNode.Parse(stream);

                return new EnumStatementNode(enumNode.Token)
                {
                    Enum = enumNode
                };
            }
            if (stream.Check(DSharpTokenType.If))
            {
                return IfStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.While))
            {
                return WhileStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.For))
            {
                return ForStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Foreach))
            {
                return ForeachStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Try))
            {
                return TryStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Return))
            {
                return ReturnStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Break))
            {
                return BreakStatementNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Continue))
            {
                return ContinueStatementNode.Parse(stream);
            }
            else if (!(stream.Check(DSharpTokenType.Identifier) && stream.Check(DSharpTokenType.LeftParen, 1)) &&
                     ObjectDeclarationNode.TryStartParseMember(stream, out var structMemberInfo))
            {
                return ParseObjectMember(structMemberInfo);
            }

            var expression = ExpressionNode.ParseExpression(stream);

            if (!stream.Check(DSharpTokenType.RightBrace))
            {
                stream.Eat(DSharpTokenType.Semicolon);
            }

            return new ExpressionStatementNode(expression.Token)
            {
                Expression = expression
            };
        }
    }
}
