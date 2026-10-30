using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// D# types provider for compiler
    /// </summary>
    public interface IDSharpTypeProvider
    {
        /// <summary>
        /// Get type based on specified context
        /// </summary>
        /// <param name="context">Context for providing type</param>
        /// <param name="parameters">Extra parameters</param>
        /// <returns>D# type</returns>
        public IDSharpType GetType(DSharpCompilerContext context, params object?[] parameters);
    }
}
