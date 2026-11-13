using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    internal class SameTypeProvider : IDSharpTypeProvider
    {
        public IDSharpType GetType(DSharpCompilerContext context, params object?[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentNullException(nameof(parameters), "Parameters can not be null");
            }
            if (parameters[0] is IDSharpType type)
            {
                return type;
            }

            throw new ArgumentException($"Parameter should provide type, but got: {parameters[0]}");
        }

        #region Static

        public static readonly SameTypeProvider Instance = new();

        #endregion
    }
}
