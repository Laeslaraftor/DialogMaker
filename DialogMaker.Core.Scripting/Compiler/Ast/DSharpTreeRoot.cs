using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Root of script file
    /// </summary>
    public class DSharpTreeRoot(string name)
    {
        /// <summary>
        /// Name of tree. It's uses to create type with entry point for this tree
        /// </summary>
        public string Name { get; } = name;
        /// <summary>
        /// List of statements that contains in script file
        /// </summary>
        public List<StatementNode> Statements { get; set; } = [];

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine($"{nameof(DSharpTreeRoot)} at {base.ToString()}");
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
