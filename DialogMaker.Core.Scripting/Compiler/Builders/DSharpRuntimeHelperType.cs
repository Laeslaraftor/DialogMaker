using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    /// <summary>
    /// Class that describes runtime helper class
    /// </summary>
    public class DSharpRuntimeHelperType(IDSharpType type, IDSharpMethodInfo createTypeMethod)
    {
        /// <summary>
        /// RuntimeHelper type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Method for creating type information instance
        /// </summary>
        public IDSharpMethodInfo CreateTypeMethod { get; } = createTypeMethod;

        #region Constants

        /// <summary>
        /// Name of method for creating type information instance
        /// </summary>
        public const string CreateTypeMethodName = "CreateType";

        #endregion

        #region Static

        /// <summary>
        /// Create description of runtime helper type
        /// </summary>
        /// <param name="assembly">Assembly that contains runtime helper class</param>
        /// <returns>Information about runtime helper class</returns>
        public static DSharpRuntimeHelperType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.Extra.RuntimeHelper);
            var createTypeMethod = type.GetMethod(CreateTypeMethodName);

            return new(type, createTypeMethod);
        }

        #endregion
    }
}
