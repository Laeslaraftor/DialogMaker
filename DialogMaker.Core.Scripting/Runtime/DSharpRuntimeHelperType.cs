namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Class that describes runtime helper class
    /// </summary>
    public class DSharpRuntimeHelperType(IDSharpType type, IDSharpMethodInfo createTypeMethod, IDSharpMethodInfo throwExecutionEngineException)
    {
        /// <summary>
        /// RuntimeHelper type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Method for creating type information instance
        /// </summary>
        public IDSharpMethodInfo CreateTypeMethod { get; } = createTypeMethod;
        /// <summary>
        /// Method for throwing execution engine exception
        /// </summary>
        public IDSharpMethodInfo ThrowExecutionEngineExceptionMethod { get; } = throwExecutionEngineException;

        #region Constants

        /// <summary>
        /// Name of method for creating type information instance
        /// </summary>
        public const string CreateTypeMethodName = "CreateType";
        /// <summary>
        /// Name of method for creating type information instance
        /// </summary>
        public const string ThrowExecutionEngineExceptionMethodName = "ThrowExecutionEngineException";

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
            var throwExecutionEngineException = type.GetMethod(ThrowExecutionEngineExceptionMethodName);

            return new(type, createTypeMethod, throwExecutionEngineException);
        }

        #endregion
    }
}
