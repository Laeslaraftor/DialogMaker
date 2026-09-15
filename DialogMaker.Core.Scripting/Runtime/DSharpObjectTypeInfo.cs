using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about object type
    /// </summary>
    /// <param name="type">Object type</param>
    /// <param name="equalsMethod">object.Equals(object, object) method</param>
    public class DSharpObjectTypeInfo(IDSharpType type, IDSharpMethodInfo toStringMethod, IDSharpMethodInfo instanceEqualsMethod, IDSharpMethodInfo getHashCodeMethod, IDSharpMethodInfo equalsMethod)
    {
        /// <summary>
        /// Object type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// object.Equals(object) method for current object with other
        /// </summary>
        public IDSharpMethodInfo InstanceEqualsMethod { get; } = instanceEqualsMethod;
        /// <summary>
        /// object.ToString() method for getting text representation of current object
        /// </summary>
        public IDSharpMethodInfo ToStringMethod { get; } = toStringMethod;
        /// <summary>
        /// object.GetHashCode() method for getting hash code of current object
        /// </summary>
        public IDSharpMethodInfo GetHashCodeMethod { get; } = getHashCodeMethod;
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
            var instanceEqualsMethod = type.GetMethods().FirstOrDefault(m => m.Name == "Equals")
                ?? throw new InvalidOperationException($"Unable to find Equals(object) method in \"{type}\"");
            var toStringMethod = type.GetMethods().FirstOrDefault(m => m.Name == "ToString")
                ?? throw new InvalidOperationException($"Unable to find ToString method in \"{type}\"");

            if (!TryFindGetHashCode(type, out var getHashCodeMethod))
            {
                throw new InvalidOperationException($"Unable to find GetHashCode method in \"{type}\"");
            }

            var equalsMethod = type.GetMethods().FirstOrDefault(m => m.Name == "Equals" && m.IsStatic)
                ?? throw new InvalidOperationException($"Unable to find static Equals(object, object) method in \"{type}\"");

            return new(type, toStringMethod, instanceEqualsMethod, getHashCodeMethod, equalsMethod);
        }

        /// <summary>
        /// Try to find GetHashCode in specified type
        /// </summary>
        /// <param name="type">Type for searching GetHashCode method</param>
        /// <param name="result">GetHashCode method that was found</param>
        /// <returns>Is GetHashCode method successfully found</returns>
        public static bool TryFindGetHashCode(IDSharpType? type, [NotNullWhen(true)] out IDSharpMethodInfo? result)
        {
            result = null;

            while (type != null)
            {
                result = type.GetMethods().FirstOrDefault(m => m.Name == "GetHashCode");

                if (result != null)
                {
                    return true;
                }

                var baseType = type.GetBaseTypes().FirstOrDefault(t => t.ObjectType != DSharpObjectType.Interface);

                if (baseType == type)
                {
                    return false;
                }

                type = baseType;
            }

            return false;
        }

        #endregion
    }
}
