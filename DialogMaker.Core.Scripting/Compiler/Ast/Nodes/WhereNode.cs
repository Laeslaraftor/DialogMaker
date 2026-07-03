using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents description of generic type
    /// </summary>
    /// <param name="token">Token that represents where keyword</param>
    public class WhereNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Type that describes
        /// </summary>
        public TypeInfoNode? Type { get; set; }
        /// <summary>
        /// Base types that type must inherit
        /// </summary>
        public List<TypeInfoNode> BaseTypes { get; set; } = [];
        /// <summary>
        /// Extra type attributes
        /// </summary>
        public DSharpGenericTypeAttributes Attributes { get; set; }

        #region Статика

        /// <summary>
        /// Parse where node starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed where node</returns>
        public static WhereNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Where);
            WhereNode result = new(token)
            {
                Type = TypeInfoNode.ParseOnlyIdentifier(stream, false)
            };

            stream.Eat(DSharpTokenType.Colon);

            while (true)
            {
                bool eatComma = false;

                if (stream.Check(DSharpTokenType.New))
                {
                    if (result.Attributes.HasFlag(DSharpGenericTypeAttributes.EmptyConstructor))
                    {
                        stream.ThrowPositionException("Multiple constructor definition");
                    }

                    stream.Eat(DSharpTokenType.New);
                    stream.Eat(DSharpTokenType.LeftParen);
                    stream.Eat(DSharpTokenType.RightParen);
                    result.Attributes |= DSharpGenericTypeAttributes.EmptyConstructor;
                    eatComma = true;
                }
                else if (stream.Check(DSharpTokenType.NotNull))
                {
                    if (result.Attributes.HasFlag(DSharpGenericTypeAttributes.NotNull))
                    {
                        stream.ThrowPositionException("Multiple notnull definition");
                    }

                    stream.Eat(DSharpTokenType.NotNull);
                    result.Attributes |= DSharpGenericTypeAttributes.NotNull;
                    eatComma = true;
                }
                else if (stream.Check(DSharpTokenType.Struct))
                {
                    if (result.Attributes.HasFlag(DSharpGenericTypeAttributes.Struct))
                    {
                        stream.ThrowPositionException("Multiple struct definition");
                    }

                    stream.Eat(DSharpTokenType.Struct);
                    result.Attributes |= DSharpGenericTypeAttributes.Struct;
                    eatComma = true;
                }
                else if (TypeInfoNode.CanParse(stream, 0))
                {
                    var baseType = TypeInfoNode.Parse(stream, true, false);
                    result.BaseTypes.Add(baseType);
                    eatComma = true;
                }

                if (!eatComma || !stream.Check(DSharpTokenType.Comma))
                {
                    break;
                }

                stream.Eat(DSharpTokenType.Comma);
            }

            return result;
        }
        /// <summary>
        /// Parse all where nodes starts with current token and write it to buffer
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="buffer">Buffer for writing parsed nodes</param>
        public static void ParseAll(AstParserStream stream, List<WhereNode> buffer)
        {
            while (stream.Check(DSharpTokenType.Where))
            {
                var node = Parse(stream);
                buffer.Add(node);
            }
        }
        /// <summary>
        /// Parse all where nodes starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>List of parsed where nodes</returns>
        public static List<WhereNode> ParseAll(AstParserStream stream)
        {
            List<WhereNode> buffer = [];
            ParseAll(stream, buffer);

            return buffer;
        }

        #endregion
    }
}
