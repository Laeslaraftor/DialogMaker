namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about D# Span`1 type
    /// </summary>
    public class DSharpSpanType(IDSharpType type, IDSharpMethodInfo arrayConstructor, IDSharpMethodInfo pointerConstructor, IDSharpIndexerInfo indexer)
    {
        /// <summary>
        /// D# Span`1 type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Constructor for creating span with array
        /// </summary>
        public IDSharpMethodInfo ArrayConstructor { get; } = arrayConstructor;
        /// <summary>
        /// Constructor for creating span with pointer and length
        /// </summary>
        public IDSharpMethodInfo PointerConstructor { get; } = pointerConstructor;
        /// <summary>
        /// Indexer for getting/setting items
        /// </summary>
        public IDSharpIndexerInfo Indexer { get; } = indexer;

        #region Static

        /// <summary>
        /// Create information about span type
        /// </summary>
        /// <param name="assembly">Assembly for searching span type</param>
        /// <returns>Information about span type</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static DSharpSpanType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.Span);
            return Create(type);
        }
        /// <summary>
        /// Create information about span type
        /// </summary>
        /// <param name="type">Span type</param>
        /// <returns>Information about span type</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static DSharpSpanType Create(IDSharpType type)
        {
            var constructors = type.GetConstructors();
            var arrayConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 1)
                ?? throw new InvalidOperationException($"Unable to find array constructor at \"{type}\"");
            var pointerConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 2)
                ?? throw new InvalidOperationException($"Unable to find pointer constructor at \"{type}\"");
            var indexer = DSharpArrayType.GetIndexer(type);

            return new(type, arrayConstructor, pointerConstructor, indexer);
        }

        /// <summary>
        /// Get array constructor at specified span type
        /// </summary>
        /// <param name="spanType">Span type for searching array constructor</param>
        /// <returns>Constructor for creating span with array</returns>
        /// <exception cref="ArgumentException"></exception>
        public static IDSharpMethodInfo GetArrayConstructor(IDSharpType spanType)
        {
            var genericParameters = spanType.GetGenericParameters();

            if (genericParameters.Length != 1)
            {
                throw new ArgumentException($"Span type \"{spanType}\" has not item type parameter", nameof(spanType));
            }

            return spanType.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1)
                ?? throw new ArgumentException($"Unable to find array constructor at \"{spanType}\"");
        }

        #endregion
    }
}
