using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Abstract syntax tree parser
    /// </summary>
    public class DSharpAstParser
    {
        /// <summary>
        /// Create new instance of ast parser
        /// </summary>
        public DSharpAstParser()
        {
            _lexer = new();
            _stream = new(_lexer);
        }

        private readonly DSharpLexer _lexer;
        private readonly AstParserStream _stream;

        #region Управление

        /// <summary>
        /// Parse source code and create script with specified name
        /// </summary>
        /// <param name="scriptName">Name of parsing script</param>
        /// <param name="sourceCode">D# source code</param>
        /// <returns>Parsed script</returns>
        public DSharpScript Parse(string scriptName, string sourceCode)
        {
            _stream.Position = 0;
            _lexer.Tokenize(sourceCode);

            return Parse(scriptName, _stream);
        }

        #endregion

        #region Static

        /// <summary>
        /// Parse source code and create script with specified name
        /// </summary>
        /// <param name="scriptName">Name of parsing script</param>
        /// <param name="sourceCode">D# source code</param>
        /// <returns>Parsed script</returns>
        public static DSharpScript ParseScript(string scriptName, string sourceCode)
        {
            DSharpLexer lexer = new();
            lexer.Tokenize(sourceCode);

            return ParseScript(scriptName, lexer);
        }
        /// <summary>
        /// Parse source code and create script with specified name
        /// </summary>
        /// <param name="scriptName">Name of parsing script</param>
        /// <param name="lexer">Lexer with tokenized source code</param>
        /// <returns>Parsed script</returns>
        public static DSharpScript ParseScript(string scriptName, DSharpLexer lexer)
        {
            return Parse(scriptName, new(lexer));
        }

        private static DSharpScript Parse(string scriptName, AstParserStream stream)
        {
            DSharpScript script = new(scriptName);
            BlockStatementNode.ParseBody(stream, DSharpStatementType.Any, script.Statements);

            return script;
        }

        #endregion
    }
}