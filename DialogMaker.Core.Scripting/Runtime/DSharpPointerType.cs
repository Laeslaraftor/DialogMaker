namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about D# pointer type
    /// </summary>
    public class DSharpPointerType(IDSharpType type, IDSharpType? valueType, IDSharpMethodInfo constructor, IDSharpFieldInfo addressField)
    {
        /// <summary>
        /// Pointer type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Type of pointer value.
        /// If this property null then pointer has unknown type (void*)
        /// </summary>
        public IDSharpType? ValueType { get; } = valueType;
        /// <summary>
        /// Is pointer has value type
        /// </summary>
        public bool IsTyped => ValueType != null;
        /// <summary>
        /// Pointer .ctor(nint)
        /// </summary>
        public IDSharpMethodInfo Constructor { get; } = constructor;
        /// <summary>
        /// Field that stores pointer address
        /// </summary>
        public IDSharpFieldInfo AddressField { get; } = addressField;

        #region Static

        /// <summary>
        /// Find pointer type in assembly and create information about it
        /// </summary>
        /// <param name="assembly">Assembly for searching pointer type</param>
        /// <param name="isTyped">Need to find typed pointer type</param>
        /// <returns>Information about pointer type that was founds</returns>
        public static DSharpPointerType Create(IDSharpAssembly assembly, bool isTyped)
        {
            var type = assembly.GetType(isTyped ? DSharpBuildInTypes.Extra.TypedPointer : DSharpBuildInTypes.Extra.Pointer);
            return Create(type);
        }
        /// <summary>
        /// Create information about pointer type
        /// </summary>
        /// <param name="type">Pointer type for creating information about it</param>
        /// <returns>Information about specified pointer type</returns>
        /// <exception cref="ArgumentException">Unable to find constructor .ctor(nint)</exception>
        public static DSharpPointerType Create(IDSharpType type)
        {
            var valueType = type.GetGenericParameters().Union(type.GetGenericTypes()).FirstOrDefault();
            var constructor = type.GetConstructors().FirstOrDefault()
                ?? throw new ArgumentException($"Unable to find constructor .ctor(nint) in \"{type}\"");
            var addressField = type.GetField("_address");

            return new(type, valueType, constructor, addressField);
        }

        /// <summary>
        /// Count pointer nesting
        /// </summary>
        /// <param name="typedPointerType">Typed pointer type info</param>
        /// <param name="type">Type for counting pointer nesting</param>
        /// <returns>Pointer nesting amount</returns>
        public static int CountNesting(IDSharpType typedPointerType, IDSharpType type)
        {
            int count = 0;

            while (type.GenericTemplate == typedPointerType)
            {
                type = type.GetGenericParameters().First();
                count++;
            }

            return count;
        }
        /// <summary>
        /// Get root type of value
        /// </summary>
        /// <param name="typedPointerType">Typed pointer type info</param>
        /// <param name="type">Type for finding root value type</param>
        /// <returns>Root value type</returns>
        public static IDSharpType GetRootValueType(IDSharpType typedPointerType, IDSharpType type)
        {
            int count = 0;

            while (type.GenericTemplate == typedPointerType)
            {
                type = type.GetGenericParameters().First();
                count++;
            }

            if (count == 0)
            {
                throw new ArgumentException($"Provided type is not pointer \"type\"", nameof(type));
            }

            return type;
        }

        #endregion
    }
}
