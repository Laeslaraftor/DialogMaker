using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Statement that represents try/catch/finally 
    /// </summary>
    /// <param name="token">Token that represents try keyword</param>
    public partial class TryStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Try statements block
        /// </summary>
        public BlockStatementNode? TryBlock { get; set; }
        /// <summary>
        /// Catch statements block
        /// </summary>
        public List<CatchBlock> CatchBlocks { get; set; } = [];
        /// <summary>
        /// Finally statements block
        /// </summary>
        public BlockStatementNode? FinallyBlock { get; set; }

        #region Static

        /// <summary>
        /// Parse try/catch/finally statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed try/catch/finally statement</returns>
        public static TryStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Try);
            TryStatementNode result = new(token)
            {
                TryBlock = BlockStatementNode.Parse(stream, DSharpStatementType.Code)
            };
            result.TryBlock.Parent = result;

            while (stream.Check(DSharpTokenType.Catch))
            {
                var catchToken = stream.Eat(DSharpTokenType.Catch);
                CatchBlock block = new(catchToken)
                {
                    Parent = result
                };

                if (stream.Check(DSharpTokenType.LeftParen))
                {
                    stream.Eat(DSharpTokenType.LeftParen);
                    block.ExceptionVariable = VariableNode.ParseVariable(stream, null, false, false, false, false);
                    block.ExceptionVariable.Parent = block;

                    stream.Eat(DSharpTokenType.RightParen);
                }

                block.Statements = BlockStatementNode.Parse(stream, DSharpStatementType.Code);
                block.Statements.Parent = block;
                result.CatchBlocks.Add(block);
            }

            if (stream.Check(DSharpTokenType.Finally))
            {
                stream.Eat(DSharpTokenType.Finally);
                result.FinallyBlock = BlockStatementNode.Parse(stream, DSharpStatementType.Code);
                result.FinallyBlock.Parent = result;
            }

            return result;
        }

        #endregion

        #region Structs

        /// <summary>
        /// Statement that represents catch block
        /// </summary>
        /// <param name="token">Token that represents catch keyword</param>
        public partial class CatchBlock(DSharpToken token) : StatementNode(token)
        {
            /// <summary>
            /// Catch statements block
            /// </summary>
            public BlockStatementNode? Statements { get; set; }
            /// <summary>
            /// Variable that contains exception instance
            /// </summary>
            public VariableNode? ExceptionVariable { get; set; }
        }

        #endregion
    }
}
