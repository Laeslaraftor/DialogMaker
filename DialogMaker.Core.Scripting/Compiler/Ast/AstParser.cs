using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Token = DialogMaker.Core.Scripting.Compiler.Lexer.DialogScriptToken;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public class AstParser(DialogScriptLexer lexer)
    {
        private readonly AstParserStream _stream = new(lexer);

        #region Управление

        public ProgramNode Parse()
        {
            _stream.Position = 0;
            ProgramNode program = new();
            BlockStatementNode.ParseBody(_stream, program.Statements);

            return program;
        }

        #endregion
    }
}