using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

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
        /// Path to source file
        /// </summary>
        public string? FilePath { get; set; }
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
            if (FilePath == null)
            {
                return Name;
            }

            return $"{Name}: {FilePath}";
        }

        #endregion
    }
}
