using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.ComponentModel;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class ProgramNode : AstNode
    {
        public ProgramNode()
        {
        }
        public ProgramNode(DialogScriptToken token) : base(token)
        {
        }

        public List<StatementNode> Statements { get; set; } = [];

        #region Управление

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine($"{nameof(ProgramNode)} at {base.ToString()}");
            builder.AppendLine($"Statements count: {Statements.Count}");
            builder.AppendLine();

            foreach (var statement in Statements)
            {
                builder.AppendLine(statement.ToString());
            }

            return builder.ToString();
        }

        #endregion
    }
}
