namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Class that provides information about string type
    /// </summary>
    /// <param name="type">String type</param>
    /// <param name="emptyConstructor">Constructor with no parameters</param>
    /// <param name="charsArrayConstructor">Constructor with single char[] parameter</param>
    /// <param name="charsSpanConstructor">Constructor with single Span<char> parameter</param>
    public class DSharpStringType(IDSharpType type, IDSharpMethodInfo emptyConstructor, IDSharpMethodInfo charsArrayConstructor, IDSharpMethodInfo charsSpanConstructor)
    {
        /// <summary>
        /// String type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Constructor with no parameters
        /// </summary>
        public IDSharpMethodInfo EmptyConstructor { get; } = emptyConstructor;
        /// <summary>
        /// Constructor with single char[] parameter
        /// </summary>
        public IDSharpMethodInfo CharsArrayConstructor { get; } = charsArrayConstructor;
        /// <summary>
        /// Constructor with single Span<char> parameter
        /// </summary>
        public IDSharpMethodInfo CharsSpanConstructor { get; } = charsSpanConstructor;

        #region Static

        /// <summary>
        /// Create information about string type
        /// </summary>
        /// <param name="assembly">Assembly that will be used for getting string type information</param>
        /// <returns>Information about string type</returns>
        /// <exception cref="ArgumentException">Unable to find empty constructor</exception>
        /// <exception cref="ArgumentException">Unable to find constructor with single char[] parameter</exception>
        public static DSharpStringType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.String);
            var charType = assembly.GetType(DSharpBuildInTypes.Char);
            var emptyConstructor = type.GetMethods().FirstOrDefault(m => m.IsStatic && m.Name == ConstructorMethodName &&
                                                                         m.GetParameters().Length == 0)
                ?? throw new ArgumentException($"Unable to find empty constructor for {type}", nameof(assembly));
            var charsArrayConstructor = type.GetMethods().FirstOrDefault(m => IsCharsGenericMethod(m, charType, "Array"))
                ?? throw new ArgumentException($"Unable to find constructor with single char[] parameter for {type}", nameof(assembly));
            var charsSpanConstructor = type.GetMethods().FirstOrDefault(m => IsCharsGenericMethod(m, charType, "Span"))
                ?? throw new ArgumentException($"Unable to find constructor with single Span<char> parameter for {type}", nameof(assembly));

            return new(type, emptyConstructor, charsArrayConstructor, charsSpanConstructor);
        }

        private static bool IsCharsGenericMethod(IDSharpMethodInfo method, IDSharpType charType, string typeName)
        {
            if (!method.IsStatic || method.Name != ConstructorMethodName)
            {
                return false;
            }

            var parameters = method.GetParameters();

            if (parameters.Length != 1 ||
                parameters[0].Type.Name != typeName)
            {
                return false;
            }

            var parameterGenericParameters = parameters[0].Type.GetGenericParameters();

            if (parameterGenericParameters.Length != 1 ||
                parameterGenericParameters[0] != charType)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Static method that that replaces string constructors
        /// </summary>
        public const string ConstructorMethodName = "Ctor";

        #endregion
    }
}
