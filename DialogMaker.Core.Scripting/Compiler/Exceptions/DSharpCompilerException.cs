using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Exception that was caught by compiler
    /// </summary>
    public class DSharpCompilerException : Exception
    {
        public DSharpCompilerException(string message, AstNode node, Exception? innerException = null) 
            : base(message, innerException)
        {
            _node = node;
        }
        public DSharpCompilerException(string message, IEnumerable<AstNode> nodes, Exception? innerException = null)
            : base(message, innerException)
        {
            Nodes = nodes;
        }

        /// <summary>
        /// Failed to compile node
        /// </summary>
        public AstNode? Node
        {
            get
            {
                if (_node != null)
                {
                    return _node;
                }

                return Nodes?.FirstOrDefault();
            }
        }
        public IEnumerable<AstNode>? Nodes { get; }

        private readonly AstNode? _node; 
    }
}
