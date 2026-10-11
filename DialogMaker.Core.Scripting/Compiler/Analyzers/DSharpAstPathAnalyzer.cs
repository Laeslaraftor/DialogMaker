using Acly.Data;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Analyzers
{
    /// <summary>
    /// D# statements analyzer that check all paths
    /// </summary>
    public static class DSharpAstPathAnalyzer
    {
        /// <summary>
        /// Check node on containing returning value on all paths
        /// </summary>
        /// <param name="node">Node for analyzing</param>
        /// <param name="result">Statements that was found during analyze</param>
        /// <returns>Is all paths contains expressions or statements that returns value</returns>
        public static bool AllPathReturns(AstNode node, out List<StatementNode> result)
        {
            return AllPathContains<ReturnStatementNode>(node, AnalyzeReturnValue, out result);
        }
        /// <summary>
        /// Check node on containing required statement on all paths
        /// </summary>
        /// <param name="node">Node for analyzing</param>
        /// <param name="result">Statements that was found during analyze</param>
        /// <returns>Is all paths contains required statement</returns>
        public static bool AllPathContains<T>(AstNode node, out List<StatementNode> result)
            where T : StatementNode
        {
            return AllPathContains<T>(node, null, out result);
        }
        /// <summary>
        /// Check node on containing required statement on all paths
        /// </summary>
        /// <param name="node">Node for analyzing</param>
        /// <param name="extraAnalyzer">Extra analyzer for statements block</param>
        /// <param name="result">Statements that was found during analyze</param>
        /// <returns>Is all paths contains required statement</returns>
        public static bool AllPathContains<T>(AstNode node, StatementsBlockAnalyzer? extraAnalyzer, out List<StatementNode> result)
            where T : StatementNode
        {
            result = [];
            DSharpAstPathVisitor<T> analyzer;
            bool isSuccess = false;
            var pool = Visitors<T>.Pool;

            lock (pool)
            {
                analyzer = pool.GetElement();
            }

            analyzer.ExtraAnalyzer = extraAnalyzer;

            try
            {
                node.Accept(analyzer);
                isSuccess = analyzer.IsSuccess;
                result.AddRange(analyzer.RequiredStatements);
            }
            finally
            {
                lock (pool)
                {
                    pool.Free(analyzer);
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Analyze statements block that represents lambda expression on returning value 
        /// </summary>
        /// <param name="blockStatement">Statements block for analyze</param>
        /// <param name="isSuccess">Is returning value expression contains</param>
        /// <returns>Selected node</returns>
        public static IEnumerable<StatementNode> AnalyzeReturnValue(BlockStatementNode blockStatement, out bool isSuccess)
        {
            isSuccess = false;

            if (blockStatement.Token.Type != DSharpTokenType.Lambda ||
                blockStatement.Statements.Count != 1)
            {
                return [];
            }

            var statement = blockStatement.Statements[0];

            if (statement is not ExpressionStatementNode expressionStatement ||
                expressionStatement.Expression == null ||
                !DSharpExpressionAnalyzer.IsMayReturnValue(expressionStatement.Expression))
            {
                return [];
            }

            isSuccess = true;

            return [statement];
        }

        #region Delegate

        /// <summary>
        /// Analyzer for statements block
        /// </summary>
        /// <param name="blockStatement">Statements block for analyze</param>
        /// <param name="isSuccess">Is analyze was finished successfully</param>
        /// <returns>Selected statements</returns>
        public delegate IEnumerable<StatementNode> StatementsBlockAnalyzer(BlockStatementNode blockStatement, out bool isSuccess);

        #endregion

        #region Classes

        private static class Visitors<T> where T : StatementNode
        {
            public static readonly ElementsPool<DSharpAstPathVisitor<T>> Pool = new();
        }

        #endregion
    }
}
