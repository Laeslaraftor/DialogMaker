using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public class AstParser(DSharpLexer lexer)
    {
        private readonly AstParserStream _stream = new(lexer);

        #region Управление

        public DSharpTreeRoot Parse(string treeName)
        {
            _stream.Position = 0;
            DSharpTreeRoot program = new(treeName);
            BlockStatementNode.ParseBody(_stream, program.Statements);

            return program;
        }

        #endregion
    }
}