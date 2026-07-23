using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    /// <summary>
    /// Class that provides information about string type
    /// </summary>
    /// <param name="type">String type</param>
    /// <param name="emptyConstructor">Constructor with no parameters</param>
    /// <param name="charsArrayConstructor">Constructor with single char[] parameter</param>
    public class DSharpStringType(IDSharpType type, IDSharpMethodInfo emptyConstructor, IDSharpMethodInfo charsArrayConstructor)
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
            var charsArrayConstructor = type.GetMethods().FirstOrDefault(m =>
            {
                if (!m.IsStatic || m.Name != ConstructorMethodName)
                {
                    return false;
                }

                var parameters = m.GetParameters();

                if (parameters.Length != 1)
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
            })
                ?? throw new ArgumentException($"Unable to find constructor with single char[] parameter for {type}", nameof(assembly));

            return new(type, emptyConstructor, charsArrayConstructor);
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
