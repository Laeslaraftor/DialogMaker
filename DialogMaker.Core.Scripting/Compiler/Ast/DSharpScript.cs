using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// D# script
    /// </summary>
    public class DSharpScript(string name)
    {
        /// <summary>
        /// Name of script. It's uses to create type with entry point for this script
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
            builder.AppendLine($"{nameof(DSharpScript)} at {base.ToString()}");
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
