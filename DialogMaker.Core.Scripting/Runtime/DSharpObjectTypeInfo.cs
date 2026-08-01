namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about object type
    /// </summary>
    /// <param name="type">Object type</param>
    /// <param name="equalsMethod">object.Equals(object, object) method</param>
    public class DSharpObjectTypeInfo(IDSharpType type, IDSharpMethodInfo equalsMethod)
    {
        /// <summary>
        /// Object type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// object.Equals(object, object) method for comparing two objects
        /// </summary>
        public IDSharpMethodInfo EqualsMethod { get; } = equalsMethod;

        #region Static

        /// <summary>
        /// Create information about object type
        /// </summary>
        /// <param name="assembly">Assembly for searching type</param>
        /// <returns>Information about object type</returns>
        /// <exception cref="InvalidOperationException">Unable to find static Equals(object, object) method</exception>
        public static DSharpObjectTypeInfo Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.Object);
            var equalsMethod = type.GetMethods().FirstOrDefault(m => m.Name == "Equals" && m.IsStatic)
                ?? throw new InvalidOperationException($"Unable to find static Equals(object, object) method in \"{type}\"");

            return new(type, equalsMethod);
        }

        #endregion
    }
}
