using DialogMaker.Core.Scripting.Compiler;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about IDisposable interface type
    /// </summary>
    /// <param name="type">IDisposable interface type</param>
    /// <param name="disposeMethod">Dispose method</param>
    public class DSharpIDisposableType(IDSharpType type, IDSharpMethodInfo disposeMethod)
    {
        /// <summary>
        /// IDisposable type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Public dispose method
        /// </summary>
        public IDSharpMethodInfo DisposeMethod { get; } = disposeMethod;

        #region Static

        /// <summary>
        /// Create information about IDisposable type from specified assembly
        /// </summary>
        /// <param name="assembly">Assembly for searching IDisposable interface type</param>
        /// <returns>Information about IDisposable</returns>
        public static DSharpIDisposableType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.Extra.IDisposable);
            var disposeMethod = GetDisposeMethod(type);

            return new(type, disposeMethod);
        }
        /// <summary>
        /// Try to get dispose method at specified type
        /// </summary>
        /// <param name="type">Type for searching dispose method</param>
        /// <param name="result">Dispose method that was found</param>
        /// <returns>Is dispose method successfully found</returns>
        public static bool TryGetDisposeMethod(IDSharpType type, [NotNullWhen(true)] out IDSharpMethodInfo? result)
        {
            result = type.GetAllMembers(m => m.Access == DSharpAccessModifier.Public &&
                                           m is IDSharpMethodInfo method &&
                                           method.Name == "Dispose" &&
                                           method.ReturnType == null &&
                                           method.GetParameters().Length == 0).Cast<IDSharpMethodInfo>().FirstOrDefault();
            return result != null;
        }
        /// <summary>
        /// Get dispose method at specified type
        /// </summary>
        /// <param name="type">Type for searching dispose method</param>
        /// <returns>Dispose method</returns>
        /// <exception cref="ArgumentException"></exception>
        public static IDSharpMethodInfo GetDisposeMethod(IDSharpType type)
        {
            if (TryGetDisposeMethod(type, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unable to find public dispose method at \"{type}\"");
        }

        #endregion
    }
}
